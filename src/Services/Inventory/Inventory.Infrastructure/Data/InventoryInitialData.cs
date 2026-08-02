using Inventory.Domain.Models;
using Inventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Data;

public static class InventoryInitialData
{
    public static async Task SeedAsync(InventoryContext context)
    {
        var allItems = GetSeedItems();

        var existingProductIds = await context.InventoryItems
            .Select(i => i.ProductId.Value)
            .ToListAsync();

        var toAdd = allItems
            .Where(i => !existingProductIds.Contains(Guid.Parse(i.ProductId)))
            .ToList();

        if (toAdd.Count == 0) return;

        var inventoryItems = toAdd.Select(i =>
        {
            var item = InventoryItem.Create(
                InventoryItemId.Of(Guid.NewGuid()),
                ProductId.Of(Guid.Parse(i.ProductId)),
                i.Name,
                i.Qty
            );
            return item;
        }).ToList();

        context.InventoryItems.AddRange(inventoryItems);
        await context.SaveChangesAsync();
    }

    private record SeedItem(string ProductId, string Name, int Qty);

    private static List<SeedItem> GetSeedItems() =>
    [
        // ── CONSOLES ──────────────────────────────────────────────────────
        new("5334c996-8457-4cf0-815c-ed2b77c4ff61", "PlayStation 5", 50),
        new("c67d6323-e8b1-4bdf-9a75-b0d0d2e7002d", "Xbox Series X", 40),
        new("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8", "Nintendo Switch OLED", 60),
        new("c0de0004-0000-0000-0000-000000000001", "PlayStation 5 Slim", 35),
        new("c0de0005-0000-0000-0000-000000000001", "Xbox Series S", 55),
        new("c0de0006-0000-0000-0000-000000000001", "Nintendo Switch Lite", 70),
        new("c0de0007-0000-0000-0000-000000000001", "Steam Deck OLED", 30),
        new("c0de0008-0000-0000-0000-000000000001", "ROG Ally X", 20),
        new("c0de0009-0000-0000-0000-000000000001", "Lenovo Legion Go", 25),
        new("c0de0010-0000-0000-0000-000000000001", "PlayStation 4 Pro", 45),
        new("c0de0011-0000-0000-0000-000000000001", "Xbox One X", 30),
        new("c0de0012-0000-0000-0000-000000000001", "Nintendo Switch V2", 50),
        new("c0de0013-0000-0000-0000-000000000001", "PlayStation VR2", 25),
        new("c0de0014-0000-0000-0000-000000000001", "Meta Quest 3", 40),
        new("c0de0015-0000-0000-0000-000000000001", "Meta Quest 3S", 50),
        new("c0de0016-0000-0000-0000-000000000001", "Atari 2600+", 20),
        new("c0de0017-0000-0000-0000-000000000001", "Sega Genesis Mini 2", 15),
        new("c0de0018-0000-0000-0000-000000000001", "PlayStation Portal", 35),
        new("c0de0019-0000-0000-0000-000000000001", "Xbox Cloud Gaming Stick", 40),
        new("c0de0020-0000-0000-0000-000000000001", "Analogue Pocket", 10),

        // ── HEADSETS ──────────────────────────────────────────────────────
        new("6ec1297b-ec0a-4aa1-be25-6726e3b51a27", "SteelSeries Arctis Nova Pro", 30),
        new("dead0002-0000-0000-0000-000000000001", "HyperX Cloud Alpha", 60),
        new("dead0003-0000-0000-0000-000000000001", "Logitech G Pro X 2", 40),
        new("dead0004-0000-0000-0000-000000000001", "Razer BlackShark V2 Pro", 45),
        new("dead0005-0000-0000-0000-000000000001", "Astro A50 Gen 5", 20),
        new("dead0006-0000-0000-0000-000000000001", "Sony INZONE H9", 35),
        new("dead0007-0000-0000-0000-000000000001", "Turtle Beach Stealth 700 Gen 3", 50),
        new("dead0008-0000-0000-0000-000000000001", "Corsair HS80 RGB Wireless", 55),
        new("dead0009-0000-0000-0000-000000000001", "ASUS ROG Delta S Animate", 25),
        new("dead0010-0000-0000-0000-000000000001", "Beyerdynamic MMX 300 Pro", 20),
        new("dead0011-0000-0000-0000-000000000001", "SteelSeries Arctis Nova 7", 45),
        new("dead0012-0000-0000-0000-000000000001", "Jabra Evolve2 55", 15),
        new("dead0013-0000-0000-0000-000000000001", "HyperX Cloud III Wireless", 50),
        new("dead0014-0000-0000-0000-000000000001", "Razer Kraken V3 HyperSense", 40),
        new("dead0015-0000-0000-0000-000000000001", "Turtle Beach Recon 500", 65),
        new("dead0016-0000-0000-0000-000000000001", "Logitech G435 Lightspeed", 55),
        new("dead0017-0000-0000-0000-000000000001", "HyperX Cloud Stinger 2", 80),
        new("dead0018-0000-0000-0000-000000000001", "Razer Kaira Pro", 45),
        new("dead0019-0000-0000-0000-000000000001", "Corsair Virtuoso RGB Elite", 30),
        new("dead0020-0000-0000-0000-000000000001", "Sennheiser GSP 600", 20),

        // ── MICE ──────────────────────────────────────────────────────────
        new("b786103d-c621-4f5a-b498-23452610f88c", "Logitech G Pro X Superlight 2", 45),
        new("face0002-0000-0000-0000-000000000001", "Razer DeathAdder V3 Pro", 50),
        new("face0003-0000-0000-0000-000000000001", "SteelSeries Prime Wireless", 35),
        new("face0004-0000-0000-0000-000000000001", "Finalmouse Starlight Pro", 15),
        new("face0005-0000-0000-0000-000000000001", "ASUS ROG Harpe Ace Aim Lab", 30),
        new("face0006-0000-0000-0000-000000000001", "Pulsar X2V2", 40),
        new("face0007-0000-0000-0000-000000000001", "Logitech G502 X Plus", 55),
        new("face0008-0000-0000-0000-000000000001", "Razer Basilisk V3 Pro", 45),
        new("face0009-0000-0000-0000-000000000001", "HyperX Pulsefire Haste 2", 60),
        new("face0010-0000-0000-0000-000000000001", "Corsair M75 Wireless", 35),
        new("face0011-0000-0000-0000-000000000001", "Glorious Model O 2 Wireless", 40),
        new("face0012-0000-0000-0000-000000000001", "Zowie EC2-CW", 30),
        new("face0013-0000-0000-0000-000000000001", "Logitech G305 Lightspeed", 80),
        new("face0014-0000-0000-0000-000000000001", "Razer Viper Mini Signature", 20),
        new("face0015-0000-0000-0000-000000000001", "Endgame Gear XM2we", 35),
        new("face0016-0000-0000-0000-000000000001", "ASUS ROG Chakram X", 25),
        new("face0017-0000-0000-0000-000000000001", "SteelSeries Aerox 5 Wireless", 30),
        new("face0018-0000-0000-0000-000000000001", "Logitech G604 Lightspeed", 45),
        new("face0019-0000-0000-0000-000000000001", "Razer Naga V2 Pro", 25),
        new("face0020-0000-0000-0000-000000000001", "Corsair Scimitar Elite", 30),

        // ── MONITORS ──────────────────────────────────────────────────────
        new("f8a1d2b3-c4e5-4f6a-8b9c-0d1e2f3a4b5c", "ASUS ROG Swift OLED PG27AQDM", 20),
        new("beef0002-0000-0000-0000-000000000001", "LG 27GP950-B", 25),
        new("beef0003-0000-0000-0000-000000000001", "Samsung Odyssey G7 32", 30),
        new("beef0004-0000-0000-0000-000000000001", "Dell Alienware AW3423DWF", 15),
        new("beef0005-0000-0000-0000-000000000001", "MSI MPG 321URX QD-OLED", 10),
        new("beef0006-0000-0000-0000-000000000001", "LG UltraGear 45GR95QE-B", 8),
        new("beef0007-0000-0000-0000-000000000001", "ASUS TUF Gaming VG27AQL3A", 40),
        new("beef0008-0000-0000-0000-000000000001", "Samsung Odyssey Neo G9 57", 5),
        new("beef0009-0000-0000-0000-000000000001", "BenQ Mobiuz EX270QM", 30),
        new("beef0010-0000-0000-0000-000000000001", "Corsair Xeneon Flex 45WQHD240", 5),
        new("beef0011-0000-0000-0000-000000000001", "LG 27GR83Q-B", 35),
        new("beef0012-0000-0000-0000-000000000001", "ViewSonic XG2431", 45),
        new("beef0013-0000-0000-0000-000000000001", "Gigabyte M32U", 25),
        new("beef0014-0000-0000-0000-000000000001", "AOC AGON Pro AG274QZM", 20),
        new("beef0015-0000-0000-0000-000000000001", "ASUS ROG Strix XG27UCS", 30),
        new("beef0016-0000-0000-0000-000000000001", "LG 32GQ850-B", 25),
        new("beef0017-0000-0000-0000-000000000001", "Dell S2522HG", 50),
        new("beef0018-0000-0000-0000-000000000001", "Acer Predator XB323QK", 20),
        new("beef0019-0000-0000-0000-000000000001", "Samsung Smart Monitor M8", 15),
        new("beef0020-0000-0000-0000-000000000001", "MSI Optix MAG274QRF-QD", 35),

        // ── KEYBOARDS ─────────────────────────────────────────────────────
        new("cafe0001-0000-0000-0000-000000000001", "Wooting 60HE+", 25),
        new("cafe0002-0000-0000-0000-000000000001", "SteelSeries Apex Pro TKL Wireless", 20),
        new("cafe0003-0000-0000-0000-000000000001", "Razer BlackWidow V4 Pro", 30),
        new("cafe0004-0000-0000-0000-000000000001", "Logitech G915 TKL Lightspeed", 35),
        new("cafe0005-0000-0000-0000-000000000001", "Corsair K100 RGB", 25),
        new("cafe0006-0000-0000-0000-000000000001", "ASUS ROG Azoth", 20),
        new("cafe0007-0000-0000-0000-000000000001", "Ducky One 3 SF", 40),
        new("cafe0008-0000-0000-0000-000000000001", "Keychron Q1 Pro", 30),
        new("cafe0009-0000-0000-0000-000000000001", "HyperX Alloy Origins 65", 50),
        new("cafe0010-0000-0000-0000-000000000001", "Razer Huntsman V3 Pro TKL", 25),
        new("cafe0011-0000-0000-0000-000000000001", "Logitech G Pro X TKL", 35),
        new("cafe0012-0000-0000-0000-000000000001", "Varmilo VA87M", 20),
        new("cafe0013-0000-0000-0000-000000000001", "Corsair K65 Plus Wireless", 30),
        new("cafe0014-0000-0000-0000-000000000001", "Endgame Gear KB65HE", 15),
        new("cafe0015-0000-0000-0000-000000000001", "Nuphy Air75 V2", 40),
        new("cafe0016-0000-0000-0000-000000000001", "ASUS ROG Falchion Ace", 30),
        new("cafe0017-0000-0000-0000-000000000001", "Razer DeathStalker V2 Pro", 25),
        new("cafe0018-0000-0000-0000-000000000001", "Anne Pro 2", 45),
        new("cafe0019-0000-0000-0000-000000000001", "Drop CTRL", 20),
        new("cafe0020-0000-0000-0000-000000000001", "SteelSeries Apex 9 TKL", 35),

        // ── CHAIRS ────────────────────────────────────────────────────────
        new("babe0001-0000-0000-0000-000000000001", "Secretlab Titan Evo 2022", 15),
        new("babe0002-0000-0000-0000-000000000001", "Herman Miller Embody Gaming", 5),
        new("babe0003-0000-0000-0000-000000000001", "Noblechairs Hero", 20),
        new("babe0004-0000-0000-0000-000000000001", "ANDA SEAT Kaiser 3 XL", 18),
        new("babe0005-0000-0000-0000-000000000001", "Secretlab Titan Evo SoftWeave", 15),
        new("babe0006-0000-0000-0000-000000000001", "Razer Iskur X XL", 10),
        new("babe0007-0000-0000-0000-000000000001", "Corsair TC500 Luxe", 20),
        new("babe0008-0000-0000-0000-000000000001", "DXRacer Formula Series", 30),
        new("babe0009-0000-0000-0000-000000000001", "Herman Miller Aeron", 8),
        new("babe0010-0000-0000-0000-000000000001", "Steelcase Leap V2", 6),
        new("babe0011-0000-0000-0000-000000000001", "Secretlab Titan Evo 2022 XL", 12),
        new("babe0012-0000-0000-0000-000000000001", "ANDA SEAT Phantom 3", 22),
        new("babe0013-0000-0000-0000-000000000001", "Noblechairs Epic", 18),
        new("babe0014-0000-0000-0000-000000000001", "AKRacing Masters Pro", 15),
        new("babe0015-0000-0000-0000-000000000001", "Homall Gaming Chair", 40),
        new("babe0016-0000-0000-0000-000000000001", "Razer Iskur V2", 12),
        new("babe0017-0000-0000-0000-000000000001", "Secretlab Classic 2020", 10),
        new("babe0018-0000-0000-0000-000000000001", "Logitech G x Herman Miller", 5),
        new("babe0019-0000-0000-0000-000000000001", "Flexispot BS14", 25),
        new("babe0020-0000-0000-0000-000000000001", "GT Racing GT099", 35),

        // ── DESKS ─────────────────────────────────────────────────────────
        new("de5c0001-0000-0000-0000-000000000001", "Secretlab Magnus Pro XL", 10),
        new("de5c0002-0000-0000-0000-000000000001", "Flexispot E7 Pro Standing Desk", 15),
        new("de5c0003-0000-0000-0000-000000000001", "UPLIFT V2 Standing Desk", 12),
        new("de5c0004-0000-0000-0000-000000000001", "L-Shaped Gaming Desk Bundle", 20),
        new("de5c0005-0000-0000-0000-000000000001", "IKEA FREDDE Gaming Desk", 30),
        new("de5c0006-0000-0000-0000-000000000001", "Atlantic Gaming Desk Pro", 25),
        new("de5c0007-0000-0000-0000-000000000001", "Flexispot E5 Standing Desk", 20),
        new("de5c0008-0000-0000-0000-000000000001", "Autonomous SmartDesk Pro", 18),
        new("de5c0009-0000-0000-0000-000000000001", "Secretlab Magnus 1500mm", 15),
        new("de5c0010-0000-0000-0000-000000000001", "Lian Li DK-05F Desk PC", 8),
        new("de5c0011-0000-0000-0000-000000000001", "Mr Ironstone L-Shaped Desk", 25),
        new("de5c0012-0000-0000-0000-000000000001", "VIVO Electric Standing Desk 55", 20),
        new("de5c0013-0000-0000-0000-000000000001", "Razer Fujin Gaming Desk", 10),
        new("de5c0014-0000-0000-0000-000000000001", "Eureka Ergonomic L60", 20),
        new("de5c0015-0000-0000-0000-000000000001", "BAUHUTTE Gaming Desk BHD-1200", 12),
        new("de5c0016-0000-0000-0000-000000000001", "ApexDesk Elite 60", 15),
        new("de5c0017-0000-0000-0000-000000000001", "IKEA LINNMON ALEX Combo", 50),
        new("de5c0018-0000-0000-0000-000000000001", "Thermaltake Pacific Gaming Desk", 10),
        new("de5c0019-0000-0000-0000-000000000001", "Uplift 4-Leg Standing Desk", 8),
        new("de5c0020-0000-0000-0000-000000000001", "Walker Edison Modern Desk", 30),

        // ── PC CASES ──────────────────────────────────────────────────────
        new("ca5e0001-0000-0000-0000-000000000001", "Lian Li PC-O11 Dynamic EVO", 25),
        new("ca5e0002-0000-0000-0000-000000000001", "Fractal Design Torrent", 20),
        new("ca5e0003-0000-0000-0000-000000000001", "Corsair 4000D Airflow", 35),
        new("ca5e0004-0000-0000-0000-000000000001", "NZXT H7 Flow", 30),
        new("ca5e0005-0000-0000-0000-000000000001", "Phanteks Enthoo Pro 2", 15),
        new("ca5e0006-0000-0000-0000-000000000001", "be quiet Silent Base 802", 18),
        new("ca5e0007-0000-0000-0000-000000000001", "Cooler Master HAF 700 EVO", 12),
        new("ca5e0008-0000-0000-0000-000000000001", "Thermaltake Tower 900", 8),
        new("ca5e0009-0000-0000-0000-000000000001", "Lian Li Q58 Mini-ITX", 20),
        new("ca5e0010-0000-0000-0000-000000000001", "Fractal Design Pop Air", 30),
        new("ca5e0011-0000-0000-0000-000000000001", "InWin 309 ARGB", 35),
        new("ca5e0012-0000-0000-0000-000000000001", "NZXT H510 Flow", 30),
        new("ca5e0013-0000-0000-0000-000000000001", "Fractal Design North XL", 15),
        new("ca5e0014-0000-0000-0000-000000000001", "Corsair iCUE 5000X RGB", 20),
        new("ca5e0015-0000-0000-0000-000000000001", "Phanteks NV7", 12),
        new("ca5e0016-0000-0000-0000-000000000001", "HYTE Y60", 18),
        new("ca5e0017-0000-0000-0000-000000000001", "Cooler Master NR200P MAX", 15),
        new("ca5e0018-0000-0000-0000-000000000001", "Silverstone FARA B1 Pro", 25),
        new("ca5e0019-0000-0000-0000-000000000001", "Lian Li Lancool III", 22),
        new("ca5e0020-0000-0000-0000-000000000001", "NZXT H9 Elite", 14),

        // ── LED / LIGHTING ────────────────────────────────────────────────
        new("1ed00001-0000-0000-0000-000000000001", "Govee Dreamview G1 TV Backlight", 40),
        new("1ed00002-0000-0000-0000-000000000001", "Nanoleaf Lines XL", 25),
        new("1ed00003-0000-0000-0000-000000000001", "Philips Hue Play Gradient Strip", 30),
        new("1ed00004-0000-0000-0000-000000000001", "Govee Neon Rope Light", 60),
        new("1ed00005-0000-0000-0000-000000000001", "Nanoleaf Shapes Hexagons", 20),
        new("1ed00006-0000-0000-0000-000000000001", "LIFX Beam", 25),
        new("1ed00007-0000-0000-0000-000000000001", "Govee Glide Hexa Pro", 30),
        new("1ed00008-0000-0000-0000-000000000001", "Elgato Key Light", 20),
        new("1ed00009-0000-0000-0000-000000000001", "Elgato Ring Light", 15),
        new("1ed00010-0000-0000-0000-000000000001", "Govee TV Strip Light Pro", 70),
        new("1ed00011-0000-0000-0000-000000000001", "Nanoleaf Canvas", 22),
        new("1ed00012-0000-0000-0000-000000000001", "Corsair iCUE LT100 Smart Tower", 18),
        new("1ed00013-0000-0000-0000-000000000001", "Philips Hue White Color A19", 80),
        new("1ed00014-0000-0000-0000-000000000001", "Govee Permanent Outdoor Lights", 25),
        new("1ed00015-0000-0000-0000-000000000001", "LIFX Z LED Strip Starter Kit", 35),
        new("1ed00016-0000-0000-0000-000000000001", "Nanoleaf 4D Screen Mirror Kit", 28),
        new("1ed00017-0000-0000-0000-000000000001", "Elgato Key Light Air", 20),
        new("1ed00018-0000-0000-0000-000000000001", "Govee RGBICWW LED Strip 16ft", 90),
        new("1ed00019-0000-0000-0000-000000000001", "Novostella 100W LED Flood", 30),
        new("1ed00020-0000-0000-0000-000000000001", "Twinkly Squares", 20),

        // ── COMPLETE SETUPS ───────────────────────────────────────────────
        new("5e700001-0000-0000-0000-000000000001", "Starter PC Setup Bundle", 15),
        new("5e700002-0000-0000-0000-000000000001", "Budget Gaming Setup 1080p", 12),
        new("5e700003-0000-0000-0000-000000000001", "Mid-Range Esports Setup", 10),
        new("5e700004-0000-0000-0000-000000000001", "Streamer Starter Bundle", 8),
        new("5e700005-0000-0000-0000-000000000001", "Console Setup PS5 Edition", 12),
        new("5e700006-0000-0000-0000-000000000001", "Console Setup Xbox Edition", 10),
        new("5e700007-0000-0000-0000-000000000001", "Dual Monitor Productivity Setup", 6),
        new("5e700008-0000-0000-0000-000000000001", "Pro Esports Setup", 8),
        new("5e700009-0000-0000-0000-000000000001", "Complete RGB Gaming Room", 5),
        new("5e700010-0000-0000-0000-000000000001", "Portable LAN Party Bundle", 20),
        new("5e700011-0000-0000-0000-000000000001", "Home Office Gaming Setup", 4),
        new("5e700012-0000-0000-0000-000000000001", "Speedrunner Setup Bundle", 10),
        new("5e700013-0000-0000-0000-000000000001", "Ultra-Wide Setup Package", 6),
        new("5e700014-0000-0000-0000-000000000001", "Budget Console Room Setup", 15),
        new("5e700015-0000-0000-0000-000000000001", "Desk Setup Refresh Bundle", 25),
        new("5e700016-0000-0000-0000-000000000001", "Creator Stream Setup", 8),
        new("5e700017-0000-0000-0000-000000000001", "Triple Monitor Setup", 5),
        new("5e700018-0000-0000-0000-000000000001", "Minimalist Gaming Setup", 10),
        new("5e700019-0000-0000-0000-000000000001", "Couch Gaming Setup", 7),
        new("5e700020-0000-0000-0000-000000000001", "Kids Gaming Starter Bundle", 20),
    ];
}
