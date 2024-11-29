namespace BusinessLogicLayer.DataAnalytics
{
    public interface IDataPrediction
    {
        void TrainModel();
        float Predict(UserPredictionRequest input);
    }
}