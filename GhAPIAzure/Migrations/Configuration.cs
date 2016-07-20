namespace GhAPIAzure.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using DbStructure.Global;

    internal sealed class Configuration : DbMigrationsConfiguration<GhAPIAzure.Models.DataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(GhAPIAzure.Models.DataContext context)
        {
            //  This method will be called after migrating to the latest version.
            Parameter[] parameters =
            {
                new Parameter { ID = 1, Name = "O₂", Unit = "‰"},
                new Parameter { ID = 2, Name = "CO₂", Unit = "‰"},
                new Parameter { ID = 3, Name = "Conductivity", Unit = "mS/m"},
                new Parameter { ID = 4, Name = "Humidity", Unit = "%"},
                new Parameter { ID = 5, Name = "Level", Unit = "%"},
                new Parameter { ID = 6, Name = "Light", Unit = "klx"},
                new Parameter { ID = 7, Name = "Ph", Unit = ""},
                new Parameter { ID = 8, Name = "Temp", Unit = "℃"},
                new Parameter { ID = 9, Name = "Water Flow", Unit = "L/m"}
            };

            context.Parameters.AddOrUpdate(h => h.ID, parameters);

            Placement[] placementTypes = {
                new Placement { ID =1, Name = "Acid Tank"},
                new Placement { ID =2, Name = "Base Tank" },
                new Placement { ID =3, Name = "Nutrient Tank" },
                new Placement { ID =4, Name = "Solution Pump" },
                new Placement { ID =5, Name = "Solution Tank" },
                new Placement { ID =6, Name = "Returning Solution" },
                new Placement { ID =7, Name = "Solution at Plants" },
                new Placement { ID =8, Name = "Substrate" },
                new Placement { ID =9, Name = "Ambient Outdoors" },
                new Placement { ID =10, Name = "Ambient Indoors" },
                new Placement { ID =11, Name = "Water Inlet" }
            };


            context.Placements.AddOrUpdate(h => h.ID, placementTypes);

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

            SensorType[] paramAtPlace =
            {
                //Acidity Subsystem
                new SensorType { ID =1, SubsystemID = 1, PlaceID = 1, ParamID = 5},
                new SensorType{ ID =2, SubsystemID = 1, PlaceID = 2, ParamID = 5},
                new SensorType { ID =3, SubsystemID = 1, PlaceID = 6, ParamID = 7},
                new SensorType { ID =4, SubsystemID = 1, PlaceID = 5, ParamID = 7},           
                //Climate Subsystem
                new SensorType { ID =5, SubsystemID = 2, PlaceID = 10, ParamID = 4},
                new SensorType { ID =6, SubsystemID = 2, PlaceID = 10, ParamID = 8},
                new SensorType { ID =7, SubsystemID = 2, PlaceID = 6, ParamID = 8},
                new SensorType { ID =8, SubsystemID = 2, PlaceID = 5, ParamID = 8},
                new SensorType { ID =9, SubsystemID = 2,  PlaceID = 8, ParamID = 8},
                //Gases Subsystem
                new SensorType { ID =10, SubsystemID = 3, PlaceID = 10, ParamID = 2},
                //Light Subsystem
                new SensorType { ID =11, SubsystemID = 4, PlaceID = 10, ParamID = 6},
                //Nutrient Subsystem
                new SensorType { ID =12, SubsystemID = 5, PlaceID = 6, ParamID = 3},
                new SensorType { ID =13, SubsystemID = 5, PlaceID = 5, ParamID = 3},
                new SensorType { ID =14, SubsystemID = 5, PlaceID = 3, ParamID = 5},
                //Oxygen Subsystem
                new SensorType { ID =15, SubsystemID = 6, PlaceID = 5, ParamID = 1},
                //Water Subsystem
                new SensorType { ID =16, SubsystemID = 7, PlaceID = 11, ParamID = 9},
                new SensorType { ID =17, SubsystemID = 7, PlaceID = 8, ParamID = 4},
                new SensorType { ID =18, SubsystemID = 7,  PlaceID = 5, ParamID = 6},
                new SensorType { ID =19, SubsystemID = 7,  PlaceID = 5, ParamID = 5}
            };

            context.SensorTypes.AddOrUpdate(h => h.ID, paramAtPlace);

            RelayType[] controlTypes =
            {
                new RelayType{ID= 1, Additive = true, Name = "Base Pump", SubsystemID = 1},
                new RelayType{ID= 2, Additive = false, Name = "Acid Pump", SubsystemID = 1},
                new RelayType{ID= 3, Additive = true, Name = "CO₂ injector", SubsystemID = 3},
                new RelayType{ID= 4, Additive = false, Name = "Ventilation", SubsystemID = 3},
                new RelayType{ID= 5, Additive = true, Name = "Lights", SubsystemID = 4},
                new RelayType{ID= 7, Additive = true, Name = "Nutrient Pump", SubsystemID = 5},
                new RelayType{ID= 8, Additive = true, Name = "Aerator", SubsystemID = 6},
                new RelayType{ID= 9, Additive = false, Name = "Water Pump", SubsystemID = 7},
            };

            context.RelayTypes.AddOrUpdate(h => h.ID, controlTypes);
            context.SaveChanges();
        }
    }
    
}
