using Microsoft.EntityFrameworkCore;
using TracePass.Domain.Entities;

namespace TracePass.Infrastructure.Data.Seeding;

public static class MasterDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        // 1. Seed Materials
        if (!await dbContext.Materials.AnyAsync())
        {
            var materials = new List<Material>
            {
                // Fibres
                new() { Name = "GOTS Certified Organic Cotton Fibre", Category = MaterialCategory.Fibre, OriginCountry = "Pakistan", Notes = "Raw combed organic cotton grown without synthetic pesticides." },
                new() { Name = "Recycled Polyester Staple Fibre", Category = MaterialCategory.Fibre, OriginCountry = "Taiwan", Notes = "Post-consumer PET bottle recycled staple fibre." },
                new() { Name = "Standard Viscose Rayon Fibre", Category = MaterialCategory.Fibre, OriginCountry = "Indonesia", Notes = "Regenerated cellulose wood pulp fibre." },
                new() { Name = "Elastane Spandex Fibre", Category = MaterialCategory.Fibre, OriginCountry = "China", Notes = "Polyether-polyurea copolymer with high elasticity." },

                // Yarns
                new() { Name = "30/1 Ne Ring-Spun Organic Cotton Yarn", Category = MaterialCategory.Yarn, OriginCountry = "Pakistan", Notes = "Carded and combed ring-spun single yarn for fine knitting." },
                new() { Name = "40/1 Ne Recycled Poly-Cotton Blend Yarn", Category = MaterialCategory.Yarn, OriginCountry = "Pakistan", Notes = "60% Organic Cotton / 40% Recycled Polyester blended yarn." },

                // Fabrics
                new() { Name = "Single Jersey Knitted Fabric 180 GSM", Category = MaterialCategory.Fabric, OriginCountry = "Pakistan", Notes = "100% Organic Cotton circular knit jersey fabric for polo shirts." },
                new() { Name = "2x2 Rib Collar & Cuff Fabric", Category = MaterialCategory.Fabric, OriginCountry = "Pakistan", Notes = "95% Cotton / 5% Elastane tubular rib fabric for necklines." },

                // Dyes
                new() { Name = "Eco-Reactive Navy Blue Dye RGB", Category = MaterialCategory.Dye, OriginCountry = "Germany", Notes = "Low-impact bifunctional reactive dye adhering to ZDHC MRSL standards." },
                new() { Name = "Reactive Black W-NN Eco Dye", Category = MaterialCategory.Dye, OriginCountry = "Switzerland", Notes = "Heavy-metal free reactive black dye with high wash fastness." },

                // Trims
                new() { Name = "Recycled Polyester Coil Zipper #5", Category = MaterialCategory.Trim, OriginCountry = "Pakistan", Notes = "OEKO-TEX Class 1 compliant recycled PET coil zipper." },
                new() { Name = "100% Organic Cotton Printed Care Labels", Category = MaterialCategory.Trim, OriginCountry = "Pakistan", Notes = "GOTS certified printed fabric brand & care wash label." }
            };

            await dbContext.Materials.AddRangeAsync(materials);
            await dbContext.SaveChangesAsync();
        }

        // 2. Seed Supplier Profiles for existing or new suppliers
        var spinningMill = await dbContext.Organizations.FirstOrDefaultAsync(o => o.Name == "Indus Valley Spinning Mills");
        if (spinningMill == null)
        {
            spinningMill = new Organization
            {
                Name = "Indus Valley Spinning Mills",
                Type = OrganizationType.Supplier,
                RegistrationNumber = "PK-REG-SPIN-010",
                Country = "Pakistan",
                ContactEmail = "sales@indusspinning.pk",
                ContactPhone = "+92418877110",
                IsVerified = true,
                IsActive = true
            };
            dbContext.Organizations.Add(spinningMill);
            await dbContext.SaveChangesAsync();
        }

        var profile = await dbContext.SupplierProfiles.Include(p => p.MaterialCategories).FirstOrDefaultAsync(p => p.OrganizationId == spinningMill.Id);
        if (profile == null)
        {
            profile = new SupplierProfile
            {
                OrganizationId = spinningMill.Id,
                Rating = 4.8m,
                Notes = "Premier GOTS-certified ring spinning supplier in Punjab.",
                CreatedAt = DateTimeOffset.UtcNow
            };
            profile.MaterialCategories.Add(new SupplierMaterialCategory { Category = MaterialCategory.Fibre });
            profile.MaterialCategories.Add(new SupplierMaterialCategory { Category = MaterialCategory.Yarn });
            dbContext.SupplierProfiles.Add(profile);
            await dbContext.SaveChangesAsync();
        }
    }
}
