using medieval_sim.core.ECS;
using medieval_sim.core.engine;
using medieval_sim.modules.world.components;
using medieval_sim.modules.world.services;
using medieval_sim.modules.world.systems;

namespace medieval_sim.modules.world;

public sealed class WorldModule : IModule
{
    public string Name => "World";
    public int LoadOrder => 0;

    public void Configure(EngineBuilder b)
    {
        b.AddSystem(new PopulationGenerationSystem())
         .AddSystem(new PassionAssignmentSystem())
         .AddSystem(new FamilyGenerationSystem())
         .AddSystem(new MarketPricingSystem())
         .AddSystem(new LeadershipSelectionSystem())
         .AddSystem(new LeadershipStipendSystem())
         .AddSystem(new WageSystem())
         .AddSystem(new FoodProductionSystem())
         .AddSystem(new FeedingSystem())
         .AddSystem(new TradeSystem());
    }

    public void Bootstrap(EngineContext ctx)
    {
        // ----- local helpers -----
        // ===== Markets helper
        EntityId NewMarket(string name)
        {
            var mid = ctx.World.Create();
            var mkt = new SettlementMarket { Name = name, PriceFood = 1.0 };
            ctx.World.Set(mid, mkt);
            mkt.SelfId = mid;
            return mid;
        }

        ctx.Register(new UniqueNameRegistry());
        
        // ===== Factions =====
        var factions = new[]
        {
            // 1) Keshari Sultanate — desert trade, drought risk
            new
            {
                Name = "Keshari Sultanate", Culture = Culture.Keshari, Treasury = 200.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.0, BufferDays = 4, MealHours = new[] { 8, 19 },
                    WillTradeExternally = true, MinRelationToTrade = -5,
                    Taxes = new TaxPolicy { TitheRate = 0.06, MarketFeeRate = 0.03, TransitTollPerUnit = 0.04 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=260, WagePool=180.0, Food=70.0, Prod=0.95,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Merchant] = 2.2;
                            sp.Weights[Profession.Caravaneer] = 1.8;
                            sp.Weights[Profession.Scribe] = 1.2;
                        })
                    },
                    new
                    {
                        Pop=210, WagePool=130.0, Food=60.0, Prod=0.9,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Fisher] = 1.2;
                            sp.Weights[Profession.Merchant] = 1.6;
                            sp.Weights[Profession.Healer] = 1.2;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Merchant]=1.15, [Profession.Caravaneer]=1.1 }
            },

            // 2) Tzanel Confederacy — river farmers, low metallurgy
            new
            {
                Name = "Tzanel Confederacy", Culture = Culture.Tzanel, Treasury = 150.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.2, BufferDays = 6, MealHours = new[] { 7, 12, 18 },
                    WillTradeExternally = true, MinRelationToTrade = -2,
                    Taxes = new TaxPolicy { TitheRate = 0.04, MarketFeeRate = 0.02, TransitTollPerUnit = 0.02 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=300, WagePool=110.0, Food=140.0, Prod=1.10,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Farmer] = 2.6;
                            sp.Weights[Profession.Healer] = 1.3;
                        })
                    },
                    new
                    {
                        Pop=220, WagePool=90.0, Food=100.0, Prod=1.05,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Farmer] = 2.2;
                            sp.Weights[Profession.Fisher] = 1.4;
                            sp.Weights[Profession.Merchant] = 1.1;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Farmer]=1.1 }
            },

            // 3) Yura Dominion — mountain mining/building, poor agri
            new
            {
                Name = "Yura Dominion", Culture = Culture.Yura, Treasury = 170.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 0.95, BufferDays = 7, MealHours = new[] { 9, 18 },
                    WillTradeExternally = true, MinRelationToTrade = -5,
                    Taxes = new TaxPolicy { TitheRate = 0.07, MarketFeeRate = 0.03, TransitTollPerUnit = 0.03 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=200, WagePool=150.0, Food=50.0, Prod=1.1,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Mason] = 1.8;
                            sp.Weights[Profession.Blacksmith] = 1.5;
                            sp.Weights[Profession.Guard] = 1.2;
                        })
                    },
                    new
                    {
                        Pop=160, WagePool=120.0, Food=40.0, Prod=1.05,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Hunter] = 1.4;
                            sp.Weights[Profession.Woodcutter] = 1.3;
                            sp.Weights[Profession.Carpenter] = 1.3;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Mason]=1.2, [Profession.Blacksmith]=1.15 }
            },

            // 4) Ashari Woodsfolk — herbs/alchemy, weak industry
            new
            {
                Name = "Ashari Woodsfolk", Culture = Culture.Ashari, Treasury = 120.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.1, BufferDays = 5, MealHours = new[] { 8, 17 },
                    WillTradeExternally = true, MinRelationToTrade = -3,
                    Taxes = new TaxPolicy { TitheRate = 0.03, MarketFeeRate = 0.02, TransitTollPerUnit = 0.01 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=180, WagePool=80.0, Food=90.0, Prod=1.1,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Healer] = 1.8;
                            sp.Weights[Profession.Hunter] = 1.5;
                            sp.Weights[Profession.Farmer] = 1.2;
                        })
                    },
                    new
                    {
                        Pop=150, WagePool=70.0, Food=70.0, Prod=1.05,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Woodcutter] = 1.6;
                            sp.Weights[Profession.Carpenter] = 1.4;
                            sp.Weights[Profession.Brewer] = 1.2;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Healer]=1.15 }
            },

            // 5) Shōkai Shogunate — balanced crafts, small farmland
            new
            {
                Name = "Shōkai Shogunate", Culture = Culture.Shokai, Treasury = 190.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.0, BufferDays = 5, MealHours = new[] { 7, 19 },
                    WillTradeExternally = true, MinRelationToTrade = 0,
                    Taxes = new TaxPolicy { TitheRate = 0.05, MarketFeeRate = 0.03, TransitTollPerUnit = 0.03 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=210, WagePool=160.0, Food=80.0, Prod=1.05,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Blacksmith] = 1.7;
                            sp.Weights[Profession.Merchant] = 1.4;
                            sp.Weights[Profession.Fisher] = 1.4;
                        })
                    },
                    new
                    {
                        Pop=170, WagePool=130.0, Food=60.0, Prod=1.0,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Scribe] = 1.3;
                            sp.Weights[Profession.Carpenter] = 1.4;
                            sp.Weights[Profession.Guard] = 1.2;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Blacksmith]=1.1, [Profession.Carpenter]=1.05 }
            },

            // 6) Norren Tundra Clans — survivalists, low agri
            new
            {
                Name = "Norren Tundra Clans", Culture = Culture.Norren, Treasury = 90.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 0.95, BufferDays = 8, MealHours = new[] { 10, 18 },
                    WillTradeExternally = true, MinRelationToTrade = -5,
                    Taxes = new TaxPolicy { TitheRate = 0.04, MarketFeeRate = 0.02, TransitTollPerUnit = 0.02 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=130, WagePool=60.0, Food=30.0, Prod=0.95,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Hunter] = 2.2;
                            sp.Weights[Profession.Fisher] = 1.5;
                            sp.Weights[Profession.Woodcutter] = 1.2;
                        })
                    },
                    new
                    {
                        Pop=110, WagePool=55.0, Food=25.0, Prod=0.95,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Guard] = 1.2;
                            sp.Weights[Profession.Carpenter] = 1.2;
                            sp.Weights[Profession.Farmer] = 0.8;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Hunter]=1.1 }
            },

            // 7) Zhurkan Steppe Horde — cavalry & raiding
            new
            {
                Name = "Zhurkan Steppe Horde", Culture = Culture.Zhurkan, Treasury = 130.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.05, BufferDays = 3, MealHours = new[] { 9, 18 },
                    WillTradeExternally = true, MinRelationToTrade = -8,
                    Taxes = new TaxPolicy { TitheRate = 0.05, MarketFeeRate = 0.04, TransitTollPerUnit = 0.02 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=160, WagePool=75.0, Food=45.0, Prod=0.98,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Soldier] = 1.6;
                            sp.Weights[Profession.Guard] = 1.4;
                            sp.Weights[Profession.Caravaneer] = 1.3;
                        })
                    },
                    new
                    {
                        Pop=140, WagePool=70.0, Food=40.0, Prod=0.98,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Hunter] = 1.4;
                            sp.Weights[Profession.Merchant] = 1.1;
                            sp.Weights[Profession.Blacksmith] = 1.2;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Soldier]=1.1, [Profession.Guard]=1.05 }
            },

            // 8) Kaenji Dominion — artificers/steam guilds
            new
            {
                Name = "Kaenji Dominion", Culture = Culture.Kaenji, Treasury = 230.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.05, BufferDays = 4, MealHours = new[] { 8, 19 },
                    WillTradeExternally = true, MinRelationToTrade = -2,
                    Taxes = new TaxPolicy { TitheRate = 0.07, MarketFeeRate = 0.05, TransitTollPerUnit = 0.03 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=220, WagePool=190.0, Food=70.0, Prod=1.15,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Blacksmith] = 2.0;
                            sp.Weights[Profession.Mason] = 1.6;
                            sp.Weights[Profession.Merchant] = 1.2;
                        })
                    },
                    new
                    {
                        Pop=180, WagePool=150.0, Food=60.0, Prod=1.1,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Carpenter] = 1.5;
                            sp.Weights[Profession.Scribe] = 1.2;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Blacksmith]=1.15, [Profession.Mason]=1.1 }
            },

            // 9) Aerani Coastlands — navigation & low tolls
            new
            {
                Name = "Aerani Coastlands", Culture = Culture.Aerani, Treasury = 160.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.1, BufferDays = 4, MealHours = new[] { 7, 12, 18 },
                    WillTradeExternally = true, MinRelationToTrade = 0,
                    Taxes = new TaxPolicy { TitheRate = 0.04, MarketFeeRate = 0.02, TransitTollPerUnit = 0.01 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=200, WagePool=130.0, Food=85.0, Prod=1.05,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Merchant] = 1.6;
                            sp.Weights[Profession.Fisher] = 1.7;
                            sp.Weights[Profession.Caravaneer] = 1.2;
                        })
                    },
                    new
                    {
                        Pop=170, WagePool=110.0, Food=70.0, Prod=1.0,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Scribe] = 1.2;
                            sp.Weights[Profession.Brewer] = 1.2;
                            sp.Weights[Profession.Carpenter] = 1.3;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Fisher]=1.1 }
            },

            // 10) Qazari Imperium — metallurgy & heavy tithe
            new
            {
                Name = "Qazari Imperium", Culture = Culture.Qazari, Treasury = 260.0,
                Policy = new FactionPolicy
                {
                    DailyFoodPerPerson = 1.05, BufferDays = 5, MealHours = new[] { 8, 18 },
                    WillTradeExternally = true, MinRelationToTrade = 0,
                    Taxes = new TaxPolicy { TitheRate = 0.12, MarketFeeRate = 0.05, TransitTollPerUnit = 0.03 }
                },
                Settlements = new []
                {
                    new
                    {
                        Pop=230, WagePool=170.0, Food=75.0, Prod=1.1,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Mason] = 1.6;
                            sp.Weights[Profession.Blacksmith] = 1.8;
                            sp.Weights[Profession.Guard] = 1.3;
                        })
                    },
                    new
                    {
                        Pop=190, WagePool=140.0, Food=60.0, Prod=1.05,
                        Name=(string?)null,
                        Spec=(Action<SettlementSpecialties>)(sp =>
                        {
                            sp.Weights[Profession.Merchant] = 1.2;
                            sp.Weights[Profession.Priest] = 1.3;
                        })
                    }
                },
                WageTweaks = new Dictionary<Profession,double> { [Profession.Blacksmith]=1.15, [Profession.Guard]=1.1 }
            },
        };

        // ----- instantiate factions -----
        var factionIds = new Dictionary<string, EntityId>();
        foreach (var f in factions)
        {
            var id = ctx.World.Create();
            var fac = new Faction { Name = f.Name, Treasury = f.Treasury, Policy = f.Policy, Culture = f.Culture };
            ctx.World.Set(id, fac);
            fac.SelfId = id;
            factionIds[f.Name] = id;
        }

        // ----- settlements & economies -----
        var settlementIds = new Dictionary<string, EntityId>();
        var factionCapitals = new Dictionary<string, Settlement>();

        // helper to add to maps & return entity id
        EntityId AddToIndex(Settlement s)
        {
            settlementIds[s.Name] = s.SelfId;
            return s.SelfId;
        }

        foreach (var f in factions)
        {
            var fid = factionIds[f.Name];
            var cult = f.Culture;

            // === CAPITAL (City + Castle) ===
            var cap = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult) + " City",
                SettlementKind.City, isCapital: true, hasCastle: true,
                wagePool: 200, foodStock: 120, productionMultiplier: 1.06,
                specialties: sp =>
                {
                    sp.Weights[Profession.Merchant] = (sp.Weights.GetValueOrDefault(Profession.Merchant) + 1.2);
                    sp.Weights[Profession.Scribe] = (sp.Weights.GetValueOrDefault(Profession.Scribe) + 0.8);
                    sp.Weights[Profession.Guard] = (sp.Weights.GetValueOrDefault(Profession.Guard) + 0.6);
                    sp.Weights[Profession.Blacksmith] = (sp.Weights.GetValueOrDefault(Profession.Blacksmith) + 0.4);
                },
                economyTweaks: e =>
                {
                    // faction-wide wage multipliers you already had
                    foreach (var kv in f.WageTweaks)
                        e.DailyWage[kv.Key] = Math.Max(0.5, e.DailyWage[kv.Key] * kv.Value);

                    // city premium
                    e.DailyWage[Profession.Merchant] = Math.Max(e.DailyWage[Profession.Merchant], 1.7);
                    e.DailyWage[Profession.Scribe] = Math.Max(e.DailyWage[Profession.Scribe], 1.6);
                }
            );
            AddToIndex(cap);
            factionCapitals[f.Name] = cap;

            // === FRONTIER KEEP (Castle) ===
            var keep = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult) + " Keep",
                SettlementKind.Castle, isCapital: false, hasCastle: true,
                wagePool: 130, foodStock: 70, productionMultiplier: 0.98,
                specialties: sp =>
                {
                    sp.Weights[Profession.Guard] = (sp.Weights.GetValueOrDefault(Profession.Guard) + 1.4);
                    sp.Weights[Profession.Soldier] = (sp.Weights.GetValueOrDefault(Profession.Soldier) + 1.2);
                    sp.Weights[Profession.Blacksmith] = (sp.Weights.GetValueOrDefault(Profession.Blacksmith) + 0.5);
                }
            );
            AddToIndex(keep);

            // === TRADE HUB (Port / Caravanserai / Town) ===
            var tradeKind = cult switch
            {
                Culture.Shokai or Culture.Aerani => SettlementKind.Port,
                Culture.Keshari or Culture.Zhurkan => SettlementKind.Caravanserai,
                _ => SettlementKind.Town
            };
            var trade = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult),
                tradeKind, isCapital: false, hasCastle: false,
                wagePool: 170, foodStock: 90, productionMultiplier: 1.05,
                specialties: sp =>
                {
                    sp.Weights[Profession.Merchant] += 1.0;
                    if (tradeKind == SettlementKind.Port)
                    {
                        sp.Weights[Profession.Sailor] += 1.5;
                        sp.Weights[Profession.Fisher] += 1.2;
                        sp.Weights[Profession.Shipwright] += 1.0;
                    }
                    if (tradeKind == SettlementKind.Caravanserai)
                    {
                        sp.Weights[Profession.Caravaneer] += 1.5;
                    }
                },
                economyTweaks: e =>
                {
                    foreach (var kv in f.WageTweaks)
                        e.DailyWage[kv.Key] = Math.Max(0.5, e.DailyWage[kv.Key] * kv.Value);
                }
            );
            AddToIndex(trade);

            // === INDUSTRIAL SITE (Mine/Town) ===
            var industrialKind = cult switch
            {
                Culture.Yura or Culture.Qazari or Culture.Kaenji => SettlementKind.Mine,
                _ => SettlementKind.Town
            };
            var works = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult),
                industrialKind, isCapital: false, hasCastle: false,
                wagePool: 150, foodStock: 75, productionMultiplier: 1.12,
                specialties: sp =>
                {
                    if (industrialKind == SettlementKind.Mine)
                    {
                        sp.Weights[Profession.Miner] += 1.8;
                        sp.Weights[Profession.Mason] += 0.8;
                        sp.Weights[Profession.Blacksmith] += 0.5;
                    }
                    else
                    {
                        sp.Weights[Profession.Carpenter] += 0.6;
                    }
                }
            );
            AddToIndex(works);

            // === VILLAGE A (Rural) ===
            var villageA = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult),
                SettlementKind.Village, isCapital: false, hasCastle: false,
                wagePool: 110, foodStock: 95, productionMultiplier: 1.10,
                specialties: sp =>
                {
                    sp.Weights[Profession.Farmer] += 1.6;
                    sp.Weights[Profession.Woodcutter] += 0.6;
                    sp.Weights[Profession.Brewer] += 0.3;
                }
            );
            AddToIndex(villageA);

            // === VILLAGE B (Rural) ===
            var villageB = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult),
                SettlementKind.Village, isCapital: false, hasCastle: false,
                wagePool: 105, foodStock: 90, productionMultiplier: 1.08,
                specialties: sp =>
                {
                    sp.Weights[Profession.Farmer] += 1.5;
                    if (cult == Culture.Tzanel) sp.Weights[Profession.Healer] += 0.4;
                    if (cult == Culture.Norren) sp.Weights[Profession.Hunter] += 0.5;
                }
            );
            AddToIndex(villageB);

            // === SHRINE / ABBEY (knowledge/faith) ===
            var shrine = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult) + " Abbey",
                SettlementKind.Abbey, isCapital: false, hasCastle: false,
                wagePool: 95, foodStock: 60, productionMultiplier: 1.00,
                specialties: sp =>
                {
                    sp.Weights[Profession.Scribe] += 0.9;
                    sp.Weights[Profession.Priest] += 0.8;
                    sp.Weights[Profession.Monk] += 0.8;
                    sp.Weights[Profession.Healer] += 0.6;
                }
            );
            AddToIndex(shrine);

            // === MARKET TOWN (general commerce) ===
            var market = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult),
                SettlementKind.Town, isCapital: false, hasCastle: false,
                wagePool: 140, foodStock: 85, productionMultiplier: 1.04,
                specialties: sp =>
                {
                    sp.Weights[Profession.Merchant] += 0.8;
                    sp.Weights[Profession.Carpenter] += 0.4;
                }
            );
            AddToIndex(market);

            // === (Optional) BORDER OUTPOST (small castle) ===
            var outpost = MakeSettlement(
                ctx, fid, cult,
                NameGenerator.NextSettlement(ctx.Rng, cult) + " Outpost",
                SettlementKind.Castle, isCapital: false, hasCastle: true,
                wagePool: 115, foodStock: 55, productionMultiplier: 0.97,
                specialties: sp =>
                {
                    sp.Weights[Profession.Guard] += 1.1;
                    sp.Weights[Profession.Soldier] += 0.9;
                    sp.Weights[Profession.Hunter] += 0.4;
                }
            );
            AddToIndex(outpost);

            #region Old

            //foreach (var s in f.Settlements)
            //{
            //    var sid = ctx.World.Create();
            //    var culture = f.Culture;
            //    var autoName = NameGenerator.NextSettlement(ctx.Rng, culture);
            //    var set = new Settlement
            //    {
            //        Name = s.Name ?? autoName,
            //        FactionId = fid,
            //        MarketId = NewMarket($"{(s.Name ?? autoName)} Market"),
            //        EconomyId = NewEconomy(ctx, $"{(s.Name ?? autoName)} Economy", e =>
            //        {
            //            e.WagePoolCoins = s.WagePool;
            //            // faction-wide wage multipliers
            //            foreach (var kv in f.WageTweaks)
            //                e.DailyWage[kv.Key] = Math.Max(0.5, e.DailyWage[kv.Key] * kv.Value);
            //        }),
            //        FoodStock = s.Food,
            //        ProductionMultiplier = s.Prod,
            //        Culture = culture
            //    };

            //    // unique name
            //    var uniq = ctx.Resolve<UniqueNameRegistry>();
            //    var rawName = NameGenerator.NextSettlement(ctx.Rng, culture);
            //    var finalName = uniq.ReserveSettlement(rawName);
            //    set.Name = finalName;

            //    ctx.World.Set(sid, set);
            //    set.SelfId = sid;

            //    AddSpecialties(ctx, sid, s.Spec);
            //    SeedHouseholds(ctx, sid, pop: s.Pop, wealthAvg: 6, wealthVar: 3);

            //    settlementIds[set.Name] = sid;
            //}
            #endregion
        }

        // ----- Relations (a few flavorful links) -----
        void Rel(string a, string b, int score)
        {
            var A = ctx.World.Get<Faction>(factionIds[a]);
            A.Relations[factionIds[b]] = score;
        }
        Rel("Keshari Sultanate", "Aerani Coastlands", +20);
        Rel("Aerani Coastlands", "Keshari Sultanate", +20);
        Rel("Tzanel Confederacy", "Ashari Woodsfolk", +10);
        Rel("Yura Dominion", "Kaenji Dominion", +15);
        Rel("Zhurkan Steppe Horde", "Keshari Sultanate", -10);
        Rel("Qazari Imperium", "Tzanel Confederacy", +5);
        Rel("Norren Tundra Clans", "Yura Dominion", +5);

        // ===== Distance graph (hours) — connect the network
        var routes = new RouteBook();
        void LinkById(EntityId a, EntityId b, int hours) => routes.Set(a, b, hours);

        // Intra-faction stitching: capital is the hub
        foreach (var f in factions)
        {
            var cap = factionCapitals[f.Name];
            // Find this faction's settlements by filtering on faction id
            var members = ctx.World.Components
                .Where(kv => kv.Value is Settlement s && s.FactionId.Equals(factionIds[f.Name]))
                .Select(kv => (Settlement)kv.Value)
                .ToList();

            // Connect capital to all members
            foreach (var s in members)
            {
                if (s.SelfId.Equals(cap.SelfId)) continue;
                var baseHours =
                    s.Kind switch
                    {
                        SettlementKind.Village => 6,
                        SettlementKind.Town => 8,
                        SettlementKind.Abbey => 9,
                        SettlementKind.Castle => 7,
                        SettlementKind.Mine => 10,
                        SettlementKind.Port => 12,
                        SettlementKind.Caravanserai => 12,
                        SettlementKind.City => 10,
                        _ => 9
                    };
                LinkById(cap.SelfId, s.SelfId, baseHours + ctx.Rng.Next(0, 4));
            }

            // A small ring among non-capitals to avoid star topology
            var nonCaps = members.Where(s => !s.IsCapital).OrderBy(s => s.Name).ToList();
            for (int i = 0; i < nonCaps.Count - 1; i++)
                LinkById(nonCaps[i].SelfId, nonCaps[i + 1].SelfId, 6 + ctx.Rng.Next(0, 6));
        }

        // Inter-faction highways: link capitals in a loop
        var caps = factionCapitals.Values.OrderBy(s => s.Name).ToList();
        for (int i = 0; i < caps.Count; i++)
        {
            var a = caps[i];
            var b = caps[(i + 1) % caps.Count];
            LinkById(a.SelfId, b.SelfId, 24 + ctx.Rng.Next(6, 12)); // 30–36 hours between capitals
        }

        ctx.Register(routes);

        // ----- schedule fairs & bad harvests everywhere -----
        foreach (var sid in settlementIds.Values)
        {
            ScheduleWeeklyFair(ctx, sid);
            ScheduleRandomBadHarvest(ctx, sid);
        }
    }

    // ----- helpers -----
    private static void SeedHouseholds(EngineContext ctx, EntityId sid, int pop, double wealthAvg, double wealthVar)
    {
        var s = ctx.World.Get<Settlement>(sid);
        var rng = ctx.Rng;
        s.Households.Clear();
        int remaining = pop;

        while (remaining > 0)
        {
            int size = Math.Max(2, Math.Min(6, rng.Next(2, 7)));
            if (size > remaining) size = remaining;

            double w = Math.Max(0.5, wealthAvg + (rng.NextDouble() * 2 - 1) * wealthVar);
            double food = Math.Max(0, rng.NextDouble() * size * 2); // up to ~2 days

            s.Households.Add(new Household { Size = size, Wealth = w, Food = food });
            remaining -= size;
        }
        s.Pop = pop;
    }

    private static void ScheduleWeeklyFair(EngineContext ctx, EntityId sid)
    {
        void ScheduleNext(DateTime from)
        {
            var next = from.Date.AddDays(7).AddHours(6); // starts 06:00 once a week
            ctx.Scheduler.Schedule(next, () =>
            {
                var s = ctx.World.Get<Settlement>(sid);
                var m = ctx.World.Get<SettlementMarket>(s.MarketId);
                m.IsFairDay = true;                 // FeedingSystem halves fee on fair days
                m.SupplyToday += Math.Max(0, s.FoodStock * 0.10); // small supply bump
                ScheduleNext(next);                 // schedule the following week
            });
        }
        ScheduleNext(ctx.Clock.Now);
    }

    private static void ScheduleRandomBadHarvest(EngineContext ctx, EntityId sid)
    {
        void Next(DateTime from)
        {
            int days = 30 + ctx.Rng.Next(0, 91);
            var start = from.Date.AddDays(days).AddHours(5);

            ctx.Scheduler.Schedule(start, () =>
            {
                var s = ctx.World.Get<Settlement>(sid);
                var old = s.ProductionMultiplier;
                s.ProductionMultiplier = 0.5; // -50%

                // end after 30 days
                ctx.Scheduler.Schedule(start.AddDays(30), () =>
                {
                    s.ProductionMultiplier = old;
                });

                Next(start);
            });
        }
        Next(ctx.Clock.Now);
    }

    private EntityId NewEconomy(EngineContext ctx, string name, Action<SettlementEconomy> cfg)
    {
        var id = ctx.World.Create();
        var e = new SettlementEconomy { Name = name };
        // default wages (tune as you like)
        foreach (Profession p in Enum.GetValues(typeof(Profession)))
            e.DailyWage[p] = p switch
            {
                // agrarian & nature
                Profession.Farmer => 1.0,
                Profession.Shepherd or Profession.Fisher or Profession.Hunter or Profession.Woodcutter => 1.1,

                // materials & industry
                Profession.Carpenter or Profession.Mason => 1.3,
                Profession.Miner => 1.35,
                Profession.Tanner or Profession.Weaver or Profession.Dyer or Profession.Potter
                    or Profession.Glassblower or Profession.Leatherworker or Profession.Tailor => 1.2,

                // metal & advanced craft
                Profession.Blacksmith or Profession.Jeweler or Profession.Shipwright or Profession.Alchemist => 1.6,
                Profession.Brewer => 1.3,

                // trade & services
                Profession.Merchant or Profession.Scribe => 1.5,
                Profession.Caravaneer or Profession.Sailor => 1.3,
                Profession.Healer => 1.5,
                Profession.Priest or Profession.Monk => 1.2,
                Profession.Bard or Profession.Cook => 1.1,

                // security & rule
                Profession.Guard or Profession.Soldier => 1.3,
                Profession.Noble => 2.0,

                _ => 1.1
            };

        cfg(e);
        ctx.World.Set(id, e);
        e.SelfId = id;
        return id;
    }

    private void AddSpecialties(EngineContext ctx, EntityId sid, Action<SettlementSpecialties> cfg)
    {
        var id = ctx.World.Create();
        var sp = new SettlementSpecialties();
        cfg(sp);
        ctx.World.Set(id, sp);

        // Link to the owning settlement
        var s = ctx.World.Get<Settlement>(sid);
        s.SpecialtiesId = id;
    }

    private Settlement MakeSettlement(EngineContext ctx, EntityId factionId, Culture culture, string rawName, SettlementKind kind,
                                      bool isCapital, bool hasCastle, double wagePool, double foodStock, double productionMultiplier,
                                      Action<SettlementSpecialties> specialties, Action<SettlementEconomy>? economyTweaks = null)
    {
        // Uniqueness (optional but nice)
        var uniq = ctx.Resolve<medieval_sim.modules.world.services.UniqueNameRegistry>();

        var finalName = uniq.ReserveSettlement(rawName);

        // Create market
        EntityId NewMarket(string name)
        {
            var mid = ctx.World.Create();
            var mkt = new SettlementMarket { Name = name, PriceFood = 1.0 };
            ctx.World.Set(mid, mkt);
            mkt.SelfId = mid;
            return mid;
        }

        // Economy
        var economyId = NewEconomy(ctx, $"{finalName} Economy", e =>
        {
            e.WagePoolCoins = wagePool;
            economyTweaks?.Invoke(e);
        });

        // Settlement component
        var sid = ctx.World.Create();
        var set = new Settlement
        {
            Name = finalName,
            FactionId = factionId,
            MarketId = NewMarket($"{finalName} Market"),
            EconomyId = economyId,
            FoodStock = foodStock,
            ProductionMultiplier = productionMultiplier,
            Culture = culture,
            IsCapital = isCapital,
            HasCastle = hasCastle,
            Kind = kind
        };
        ctx.World.Set(sid, set);
        set.SelfId = sid;

        // Specialties
        AddSpecialties(ctx, sid, specialties);

        // Seed people (size scaled by kind)
        int pop = kind switch
        {
            SettlementKind.City or SettlementKind.Port => 260 + ctx.Rng.Next(40, 120),
            SettlementKind.Castle => 140 + ctx.Rng.Next(20, 60),
            SettlementKind.Mine => 180 + ctx.Rng.Next(40, 80),
            SettlementKind.Caravanserai => 200 + ctx.Rng.Next(30, 90),
            _ => 160 + ctx.Rng.Next(20, 80)
        };
        SeedHouseholds(ctx, sid, pop, wealthAvg: 6, wealthVar: 3);

        // Weekly fair + random bad harvest hooks you already have
        ScheduleWeeklyFair(ctx, sid);
        ScheduleRandomBadHarvest(ctx, sid);

        return set;
    }

}