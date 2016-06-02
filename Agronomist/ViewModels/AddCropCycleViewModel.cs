using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Agronomist.Models;


namespace Agronomist.ViewModels
{
    public class AddCropCycleViewModel : ViewModelBase
    {
        MainDbContext _db = new MainDbContext();

        private HashSet<PlaceTuple> _placeList = new HashSet<PlaceTuple>();
        private PlaceTuple _chosenPlace = null;
        private bool _chosenIsVacant = false; 
        private Action<string> updateEvent; 

        public AddCropCycleViewModel()
        {
            var places = _db.Locations.Where(loc => loc.Devices.Count > 0).Include(loc => loc.CropCycles).ToList();

            foreach (Location loc in places)
            {
                PlaceTuple tuple = new PlaceTuple
                {  Location = loc }; 

                CropCycle runningCropCycle = loc.CropCycles.FirstOrDefault(cc => cc.EndDate == null);
                if (runningCropCycle != null)
                {
                    tuple.DisplayName = loc.Name + " - already monitoring " + runningCropCycle.CropTypeName + " of variety " + runningCropCycle.CropVariety;
                    tuple.IsVacant = false;
                }
                else
                {
                    tuple.DisplayName = loc.Name + " - Avaliable";
                    tuple.IsVacant = true; 
                }
                PlaceList.Add(tuple);
            }
            OnPropertyChanged("PlaceList"); 
        }





        public HashSet<PlaceTuple> PlaceList
        { get {
                return _placeList; }
            private set {
                _placeList = value;
                OnPropertyChanged(); 
            }
        }

        /// <summary>
        /// This is the selection of the user 
        /// </summary>
        public Object SelectedPlace {
            get { return _chosenPlace; }
            set
            {
                _chosenPlace = (PlaceTuple)value;
                ChosenIsVacant = _chosenPlace.IsVacant; 
            }
        }

        public Boolean ChosenIsVacant
        {
            get { return _chosenIsVacant; }
            set {
                _chosenIsVacant = value;
                OnPropertyChanged();                 
            }
        }

        /// <summary>
        /// Used as a grouping for UI to blabber about
        /// </summary>
        public class PlaceTuple : ViewModelBase
        {
            private string _displayName;
            private Location _location;
            private bool _isVacant; 


            public string DisplayName {
                get { return _displayName; }
                set {_displayName = value;
                    OnPropertyChanged(); 
                }
            }
            public Location Location {
                get { return _location; }
                set
                {
                    _location = value;
                    OnPropertyChanged(); 
                }
            }
            public Boolean IsVacant {
                get { return _isVacant; }
                set
                {
                    _isVacant = value;
                    OnPropertyChanged();
                }
            }
        }



    }

    
}
