namespace GhAPIAzure.Migrations
{
    using DatabasePOCOs.Global;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GhAPIAzure.Models.DbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(GhAPIAzure.Models.DbContext context)
        {
            //  This method will be called after migrating to the latest version.
            Parameter[] parameters =
            {
                new Parameter { ID = 1, Name = "O₂", Unit = "‰"},
                new Parameter { ID = 2, Name = "CO₂", Unit = "‰"},
                new Parameter { ID = 3, Name = "Conductivity", Unit = "mS/m"},
                new Parameter { ID = 4, Name = "Humidity", Unit = "%"},
                new Parameter { ID = 5, Name = "Level", Unit = "%"},
                new Parameter { ID = 6, Name = "Light", Unit = "Lux"},
                new Parameter { ID = 7, Name = "Ph", Unit = ""},
                new Parameter { ID = 8, Name = "Temp", Unit = "℃"},
                new Parameter { ID = 9, Name = "Water Flow", Unit = "L/m"}
            };

            context.Parameters.AddOrUpdate(h => h.ID, parameters);

            PlacementType[] placementTypes = {
                new PlacementType { ID =1, Name = "Acid Tank"},
                new PlacementType { ID =2, Name = "Base Tank" },
                new PlacementType { ID =3, Name = "Nutrient Tank" },
                new PlacementType { ID =4, Name = "Solution Pump" },
                new PlacementType { ID =5, Name = "Solution Tank" },
                new PlacementType { ID =6, Name = "Returning Solution" },
                new PlacementType { ID =7, Name = "Solution at Plants" },
                new PlacementType { ID =8, Name = "Substrate" },
                new PlacementType { ID =9, Name = "Ambient Outdoors" },
                new PlacementType { ID =10, Name = "Ambient Indoors" },
                new PlacementType { ID =11, Name = "Water Inlet" }
            };


            context.PlacementTypes.AddOrUpdate(h => h.ID, placementTypes);

            Subsystem[] subsystems =
            {
                new Subsystem { ID =1, Name = "Acidity"},
                new Subsystem { ID =2, Name = "Climate"},
                new Subsystem { ID =3, Name = "CO₂"},
                new Subsystem { ID =4, Name = "Light"},
                new Subsystem { ID =5, Name = "Nutrient"},
                new Subsystem { ID =6, Name = "Oxygen"},
                new Subsystem { ID =7, Name = "Water"},
            };

            context.Subsystems.AddOrUpdate(h => h.ID, subsystems);

            ParamAtPlace[] paramAtPlace =
            {
                //Acidity Subsystem
                new ParamAtPlace { ID =1, SubsystemID = 1, PlaceID = 1, ParamID = 5},
                new ParamAtPlace { ID =2, SubsystemID = 1, PlaceID = 2, ParamID = 5},
                new ParamAtPlace { ID =3, SubsystemID = 1, PlaceID = 6, ParamID = 7},
                new ParamAtPlace { ID =4, SubsystemID = 1, PlaceID = 5, ParamID = 7},           
                //Climate Subsystem
                new ParamAtPlace { ID =5, SubsystemID = 2, PlaceID = 10, ParamID = 4},
                new ParamAtPlace { ID =6, SubsystemID = 2, PlaceID = 10, ParamID = 8},
                new ParamAtPlace { ID =7, SubsystemID = 2, PlaceID = 6, ParamID = 8},
                new ParamAtPlace { ID =8, SubsystemID = 2, PlaceID = 5, ParamID = 8},
                new ParamAtPlace { ID =9, SubsystemID = 2,  PlaceID = 8, ParamID = 8},
                //Gases Subsystem
                new ParamAtPlace { ID =10, SubsystemID = 3, PlaceID = 10, ParamID = 2},
                //Light Subsystem
                new ParamAtPlace { ID =11, SubsystemID = 4, PlaceID = 10, ParamID = 6},
                //Nutrient Subsystem
                new ParamAtPlace { ID =12, SubsystemID = 5, PlaceID = 6, ParamID = 3},
                new ParamAtPlace { ID =13, SubsystemID = 5, PlaceID = 5, ParamID = 3},
                new ParamAtPlace { ID =14, SubsystemID = 5, PlaceID = 3, ParamID = 5},
                //Oxygen Subsystem
                new ParamAtPlace { ID =15, SubsystemID = 6, PlaceID = 5, ParamID = 1},
                //Water Subsystem
                new ParamAtPlace { ID =16, SubsystemID = 7, PlaceID = 11, ParamID = 9},
                new ParamAtPlace { ID =17, SubsystemID = 7, PlaceID = 8, ParamID = 4},
                new ParamAtPlace { ID =18, SubsystemID = 7,  PlaceID = 5, ParamID = 6}
            };

            context.ParamsAtPlaces.AddOrUpdate(h => h.ID, paramAtPlace);

            ControlType[] controlTypes =
            {
                new ControlType{ID= 1, Additive = true, Name = "Base Pump", SubsystemID = 1},
                new ControlType{ID= 2, Additive = false, Name = "Acid Pump", SubsystemID = 1},
                new ControlType{ID= 3, Additive = true, Name = "CO₂ injector", SubsystemID = 3},
                new ControlType{ID= 4, Additive = false, Name = "Ventilation", SubsystemID = 3},
                new ControlType{ID= 5, Additive = true, Name = "Lights", SubsystemID = 4},
                new ControlType{ID= 7, Additive = true, Name = "Nutrient Pump", SubsystemID = 5},
                new ControlType{ID= 8, Additive = true, Name = "Aerator", SubsystemID = 6},
                new ControlType{ID= 9, Additive = false, Name = "Water Pump", SubsystemID = 7},
            };

            context.ControlTypes.AddOrUpdate(h => h.ID, controlTypes);
            context.SaveChanges();
        }
    }
}
