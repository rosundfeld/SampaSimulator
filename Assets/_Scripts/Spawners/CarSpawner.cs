public class CarSpawner : BaseSpawner
{
    private const string CAR_TAG = "Car";

    protected override string GetSpawnedTag()
    {
        return CAR_TAG;
    }
}
