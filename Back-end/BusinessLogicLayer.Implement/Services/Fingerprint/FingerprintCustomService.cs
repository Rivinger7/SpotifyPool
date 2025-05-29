using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using SoundFingerprinting.Audio;
using SoundFingerprinting.Builder;
using SoundFingerprinting.Data;
using SoundFingerprinting.InMemory;
using SoundFingerprinting.Query;
using System.IO.Compression;
using Utility.Coding;
using Xabe.FFmpeg;
using Path = System.IO.Path;

namespace BusinessLogicLayer.Implement.Services.Fingerprint
{
    public class FingerprintCustomService(IUnitOfWork unitOfWork, IFFmpegService fFmpegService)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IFFmpegService _fFmpegService = fFmpegService;

        public async Task SaveFingerprintToMongo(IFormFile audioFile)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string rootFolder = "audio_processing";
            string inputIntermediateFolder = "input_temp_audio";
            string ouputIntermediateFolder = "output_wav_audio";

            SoundFingerprintingAudioService audioService = new();

            (string tempPath, long? bitrate) = await _fFmpegService.ConvertToWavFileAsync(audioFile, basePath, rootFolder, inputIntermediateFolder, ouputIntermediateFolder);

            AVHashes hashes = await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(tempPath)
                .UsingServices(audioService)
                .Hash();

            if(File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            AudioFingerprint doc = new()
            {
                CompressedFingerprints = hashes.Audio.Select(h => CompressIntArray(h.HashBins)).ToList(),
                SequenceNumbers = hashes.Audio.Select(h => h.SequenceNumber).ToList(),
                StartsAt = hashes.Audio.Select(h => h.StartsAt).ToList(),
                OriginalPoints = hashes.Audio.Select(h => h.OriginalPoint).ToList(),
                Duration = hashes.Audio.DurationInSeconds,
            };

            await _unitOfWork.GetCollection<AudioFingerprint>().InsertOneAsync(doc);

            return;
        }

        public async Task<double> CompareWithDatabase(IFormFile inputFile)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string rootFolder = "audio_processing";
            string inputIntermediateFolder = "input_temp_audio";
            string ouputIntermediateFolder = "output_wav_audio";

            SoundFingerprintingAudioService audioService = new();

            (string wavPath, long? bitrate) = await _fFmpegService.ConvertToWavFileAsync(inputFile, basePath, rootFolder, inputIntermediateFolder, ouputIntermediateFolder);

            await FingerprintCommandBuilder.Instance
                .BuildFingerprintCommand()
                .From(wavPath)
                .UsingServices(audioService)
                .Hash();

            List<AudioFingerprint> allDocs = await _unitOfWork.GetCollection<AudioFingerprint>().Find(_ => true).ToListAsync();

            double bestConfidence = 0;
            InMemoryModelService tempModelService = new();

            foreach (AudioFingerprint doc in allDocs)
            {

                TrackInfo track = new(doc.Id, "temp", "unknown");

                HashedFingerprint[] hashesFromDb = new HashedFingerprint[doc.CompressedFingerprints.Count];
                for (int i = 0; i < doc.CompressedFingerprints.Count; i++)
                {
                    hashesFromDb[i] = new HashedFingerprint(
                        DecompressToIntArray(doc.CompressedFingerprints[i]),
                        doc.SequenceNumbers[i],
                        doc.StartsAt[i],
                        doc.OriginalPoints[i]);
                }

                Hashes audioHashes = new(hashesFromDb, doc.CompressedFingerprints.Count * 0.928, MediaType.Audio);

                AVHashes avHashes = new(audioHashes, null);

                tempModelService.Insert(track, avHashes);

                AVQueryResult queryResult = await QueryCommandBuilder.Instance
                    .BuildQueryCommand()
                    .From(wavPath)
                    .UsingServices(tempModelService, audioService)
                    .Query();

                if (queryResult.BestMatch?.Audio.Confidence * 100 > bestConfidence)
                {
                    bestConfidence = queryResult.BestMatch.Audio.Confidence * 100;
                }
            }

            if (File.Exists(wavPath))
            {
                File.Delete(wavPath);
            }

            return bestConfidence;
        }

        //public async Task<double> CompareWithDatabase(IFormFile inputFile)
        //{
        //    var audioService = new SoundFingerprintingAudioService();
        //    //var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + Path.GetExtension(inputFile.FileName));

        //    //await using (var fs = new FileStream(tempPath, FileMode.Create))
        //    //    await inputFile.CopyToAsync(fs);

        //    var wavPath = await _fFmpegService.ConvertToWavFileAsync(inputFile);

        //    var hashes = await FingerprintCommandBuilder.Instance
        //        .BuildFingerprintCommand()
        //        .From(wavPath)
        //        .UsingServices(audioService)
        //        .Hash();

        //    List<OTP> allDocs = await unitOfWork.GetCollection<OTP>().Find(_ => true).ToListAsync();

        //    double best = 0;

        //    foreach (OTP doc in allDocs)
        //    {
        //        var tempModelService = new InMemoryModelService();

        //        var track = new TrackInfo(doc.Id, "temp", "unknow");
        //        HashedFingerprint[] hashesFromDb = new HashedFingerprint[doc.FingerPrints.Count];

        //        for (int i = 0; i < doc.FingerPrints.Count; i++)
        //        {
        //            hashesFromDb[i] = new HashedFingerprint(doc.FingerPrints[i].ToArray(), doc.SequenceNumbers[i], doc.StartsAt[i], doc.OriginalPoints[i]);
        //        }

        //        // Dùng đúng constructor mới của Hashes
        //        var audioHashes = new Hashes(hashesFromDb, doc.FingerPrints.Count * 0.928, MediaType.Audio);
        //        var avHashes = new AVHashes(audioHashes, null);

        //        tempModelService.Insert(track, avHashes);

        //        var queryResult = await QueryCommandBuilder.Instance
        //            .BuildQueryCommand()
        //            .From(wavPath)
        //            .UsingServices(tempModelService, audioService)
        //            .Query();

        //        if (queryResult.BestMatch?.Audio.Confidence * 100 > best)
        //        {
        //            best = queryResult.BestMatch.Audio.Confidence * 100;
        //        }
        //    }

        //    File.Delete(tempPath);
        //    File.Delete(wavPath);

        //    return best;
        //}

        private byte[] CompressIntArray(int[] data)
        {
            using var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionLevel.Optimal))
            using (var bw = new BinaryWriter(gzip))
            {
                foreach (var val in data)
                    bw.Write(val);
            }
            return ms.ToArray();
        }

        private int[] DecompressToIntArray(byte[] compressed)
        {
            using var ms = new MemoryStream(compressed);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            using var br = new BinaryReader(gzip);
            var list = new List<int>();
            try
            {
                while (true)
                    list.Add(br.ReadInt32());
            }
            catch (EndOfStreamException)
            {
                return list.ToArray();
            }
        }
    }
}
