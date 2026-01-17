namespace MVC.Models.Services;

public class FileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _uploadsFolderPath;

    public FileStorage(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        _uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "images");
    }

    private string GetFilePath(string fileName)
    {
        // Путь будет выглядеть так: wwwroot/uploads/images/a/b/ab1234.jpg
        return Path.Combine(_uploadsFolderPath, fileName[0].ToString(), fileName[1].ToString(), fileName);
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = GetFilePath(newFileName);
        var directoryPath = Path.GetDirectoryName(filePath);

        if (directoryPath != null && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return newFileName;
    }

    public Task DeleteFileAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return Task.CompletedTask;
        
        var filePath = GetFilePath(fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        return Task.CompletedTask;
    }
}