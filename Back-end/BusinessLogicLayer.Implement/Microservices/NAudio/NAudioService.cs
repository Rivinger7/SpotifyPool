using NAudio.Lame;
using NAudio.Wave;

namespace BusinessLogicLayer.Implement.Microservices.NAudio
{
    public static class NAudioService
    {
        public static void TrimAudioFile(out int duration, string inputPath, string outputPath, TimeSpan endTime)
        {
            // mở file mp3 lưu ở wwwroot/input để đọc
            using (var reader = new AudioFileReader(inputPath))
            {
                // lấy tổng thời gian nhạc trên file mp3
                duration = (int)reader.TotalTime.TotalSeconds * 1000;

                //ghi âm thanh vào file mp3 dưới dạng mã hóa LAME (đường dẫn đầu ra, định dạng sóng(chứa thông tin tần số,...), độ nén sử dụng mã hóa mp3)
                using (LameMP3FileWriter writer = new LameMP3FileWriter(outputPath, reader.WaveFormat, LAMEPreset.STANDARD))
                {
                    //số byte dựa trên tốc độ trung bình mỗi giây
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    //tính vị trí cần kết thúc theo số byte
                    int endBytes = (int)endTime.TotalMilliseconds * bytesPerMillisecond;
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;

                    TrimAudioFile(reader, writer, endBytes);
                }
            }
        }

        private static void TrimAudioFile(AudioFileReader reader, LameMP3FileWriter writer, int endBytes)
        {
            byte[] buffer = new byte[1024];

            //đọc file mp3 từ vị trí hiện tại đến vị trí byte cần kết thúc
            while (reader.Position < endBytes)
            {
                //khoảng cách byte từ vị trí hiện tại đến vị trí byte kết thúc, nếu khoảng cách > 0 thì vẫn còn dữ liệu để đọc
                int bytesRequired = (int)(endBytes - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length); // đảm bảo bytesToRead không vượt quá buffer
                    int bytesRead = reader.Read(buffer, 0, bytesToRead); //lưu dữ liệu đã được đọc từ buffer vào bytesToRead
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead); //ghi dữ liệu từ buffer vào writer
                    }
                }
            }
        }
    }
}
