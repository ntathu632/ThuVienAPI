using LapTrinhWeb2_API.Models.Domain;

namespace LapTrinhWeb2_API.Repositories
{
    public interface IImageRepository
    {
        Image Upload(Image image);
        List<Image> GetAllInfoImages();
        (byte[], string, string) DownloadFile(int Id);
    }
}