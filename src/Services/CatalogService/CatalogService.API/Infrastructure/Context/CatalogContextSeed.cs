using CatalogService.Api.Core.Domain;
using Polly;
using System.Data.SqlClient;
using System.Globalization;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retry}", retry);
                }
                );

            var setupDirectoryPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirectoryPath, picturePath, logger));
        }

        private async Task ProcessSeeding(CatalogContext context, string setupDirectoryPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirectoryPath));

                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupDirectoryPath));

                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirectoryPath, context));

                await context.SaveChangesAsync();

                GetCatalogItemPictures(setupDirectoryPath, picturePath);
            }
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
            {

                return new List<CatalogBrand>()
                {
                     new CatalogBrand (){ Brand="Azure"},
                     new CatalogBrand (){ Brand=".NET"},
                     new CatalogBrand (){ Brand = "Visual Studio"},
                     new CatalogBrand (){ Brand = "SQL Server"},
                     new CatalogBrand (){ Brand="Other"}
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogBrands.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogBrands();
            }

            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(i => new CatalogBrand()
            {
                Brand = i.Trim('"'),
            }).Where(i => i != null);

            return list ?? GetPreconfiguredCatalogBrands();

        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
            {

                return new List<CatalogType>()
                {
                     new CatalogType (){ Type="Mug"},
                     new CatalogType (){ Type="T-Shirt"},
                     new CatalogType (){ Type="Sheet"},
                     new CatalogType (){ Type="USB Memory Stick"},

                };
            }

            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogTypes();
            }

            var fileContent = File.ReadAllLines(fileName);

            var list = fileContent.Select(i => new CatalogType()
            {
                Type = i.Trim('"'),
            }).Where(i => i != null);

            return list ?? GetPreconfiguredCatalogTypes();

        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfiguredItems()
            {
                return new List<CatalogItem>()
                {
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=2,AvailableStock=100,Description=".NET Bot Black Hoodie",Name=".NET Bot Black Hoodie",Price=15,PictureFileName="1.png", OnReorder=true},
                   new CatalogItem {CatalogTypeId=1,CatalogBrandId=2,AvailableStock=100,Description=".NET Black & White Mug",Name=".NET Black & White Mug",Price=15,PictureFileName="2.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=5,AvailableStock=100,Description="Prism White T-Shirt",Name="Prism White T-Shirt",Price=15,PictureFileName="3.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=2,AvailableStock=100,Description=".NET Foundation T-Shirt",Name=".NET Foundation T-Shirt",Price=15,PictureFileName="4.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=3,CatalogBrandId=5,AvailableStock=100,Description="Roslyn Red Sheet",Name="Roslyn Red Sheet",Price=15,PictureFileName="5.png", OnReorder=true},
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=2,AvailableStock=100,Description=".NET Blue Hoodie",Name=".NET Blue Hoodie",Price=15, PictureFileName="6.png",OnReorder=false},
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=5,AvailableStock=100,Description="Roslyn Red T-Shirt",Name="Roslyn Red T-Shirt",Price=15, PictureFileName = "7.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=5,AvailableStock=100,Description="Kudu Purple Hoodie",Name="Kudu Purple Hoodie",Price=15,PictureFileName="8.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=1,CatalogBrandId=5,AvailableStock=100,Description="Cup<T> White Mug",Name="Cup<T> White Mug",Price=15,PictureFileName="9.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=3,CatalogBrandId=2,AvailableStock=100,Description=".NET Foundation Sheet",Name=".NET Foundation Sheet",Price=15,PictureFileName="10.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=3,CatalogBrandId=2,AvailableStock=100,Description="Cup<T> Sheet",Name="Cup<T> Sheet",Price=15,PictureFileName="11.png", OnReorder=false},
                   new CatalogItem {CatalogTypeId=2,CatalogBrandId=5,AvailableStock=100,Description="Prism White TShirt",Name="Prism White TShirt",Price=15,PictureFileName="12.png", OnReorder=true}

                };
            }

            string fileName = Path.Combine(contentPath, "CatalogItems.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredItems();
            }

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(cb => cb.Brand, cb => cb.Id);

            var fileContent = File.ReadAllLines(fileName)
                .Skip(1) // skip header row
                .Select(i => i.Split(','))
                .Select(i => new CatalogItem()
                {
                    CatalogTypeId = catalogTypeIdLookup[i[0]],
                    CatalogBrandId = catalogBrandIdLookup[i[1]],
                    Description = i[2].Trim('"').Trim(),
                    Name = i[3].Trim('"').Trim(),
                    Price = Decimal.Parse(i[4].Trim('"').Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    PictureFileName = i[5].Trim('"').Trim(),
                    AvailableStock = string.IsNullOrEmpty(i[6]) ? 0 : int.Parse(i[6]),
                    OnReorder = Convert.ToBoolean(i[7])

                });

            return fileContent;

        }

        private void GetCatalogItemPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";

            if (picturePath != null)
            {
                DirectoryInfo directory = new DirectoryInfo(picturePath);

                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                //string zipFileCatalogItemPictures = Path.Combine(contentPath, "CatalogItems.zip");
                //ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
            }
        }
    }
}
