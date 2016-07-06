using DbStructure.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quickbird.Models;
using DbStructure;
using Quickbird.Util;


namespace Quickbird.ViewModels
{
    using Models;
    using Util;

    public class ArchiveViewModel : ViewModelBase
    {
        private Action<string> _updateAction;
        private CropCyclePresenter _selectedCropCycle; 

        public ArchiveViewModel()
        {
            _updateAction = Update; 
            Messenger.Instance.TablesChanged.Subscribe(_updateAction);
            Update(string.Empty); 
        }

        public async void Update(string input)
        {
            MainDbContext db = new MainDbContext();
            var cropCycles = await db.CropCycles.Where(cc => cc.EndDate != null && cc.EndDate < DateTimeOffset.Now).Include(cc => cc.Location).ToListAsync();

            for(int i=0; i< cropCycles.Count; i++)
            {
                bool matchFound = false; 
                for(int m=0; m < CropCycles.Count; m++)
                {
                    if(cropCycles[i].ID == CropCycles[m].CropCycle.ID)
                    {
                        matchFound = true;
                        CropCycles[m] = new CropCyclePresenter(cropCycles[i]);
                    }
                }
                if (matchFound == false)
                {
                    CropCycles.Add(new CropCyclePresenter(cropCycles[i]));
                }
            }
            CropCycles.OrderBy(cc => cc.StartDate); 
            //Group the Crop Cycles
            OnPropertyChanged(nameof(CropCycles));
        }

        public ObservableCollection<CropCyclePresenter> CropCycles { get; set; } = new ObservableCollection<CropCyclePresenter>();

        public List<int> craps { get; set; } = new List<int>(new[] { 1, 2, 3, 4 }); 

        public object SelectedCropCycle
        {
            get
            {
                return _selectedCropCycle; 
            }
            set
            {
                _selectedCropCycle = (CropCyclePresenter)value;
                OnPropertyChanged("StartDate");
                OnPropertyChanged("EndDate");
                OnPropertyChanged("Duration");
                OnPropertyChanged("Yield");
                OnPropertyChanged("Variety");
                OnPropertyChanged("CropType");
            }
        }

        public string StartDate
        {
            get { if (_selectedCropCycle == null)
                    return " --- ";
                else
                    return _selectedCropCycle.CropCycle.StartDate.ToString("dd MMM yyyy");
            }
        }

        public string EndDate
        {
            get {
                if (_selectedCropCycle == null)
                    return " --- ";
                else
                    return _selectedCropCycle.CropCycle.EndDate?.ToString("dd MMM yyyy") ?? "On Going";  }
        }

        public string Duration
        {
            get {
                if (_selectedCropCycle == null)
                    return " --- ";
                else
                {
                    TimeSpan result = ((_selectedCropCycle.CropCycle.EndDate ?? DateTimeOffset.Now) -
                  _selectedCropCycle.CropCycle.StartDate);
                    return Math.Round(result.TotalDays, 1).ToString() + " days";
                }
            }
        }

        public string Yield
        {
            get
            {
                if (_selectedCropCycle == null)
                    return " --- ";
                else
                    return _selectedCropCycle.CropCycle.Yield.ToString() + " kg"; 
            }
        }

        public string Variety
        {
            get
            {
                return _selectedCropCycle?.CropCycle.CropVariety ?? " --- ";
            }
        }

        public string CropType
        {
            get
            {
                return _selectedCropCycle?.CropCycle.CropTypeName ?? " --- ";
            }
        }

        public class CropCyclePresenter
        {
            public CropCyclePresenter(CropCycle inCropCycle)
            {
                this.CropCycle = inCropCycle;
                StartDate = inCropCycle.StartDate.ToString("dd MMM");
                EndDate = inCropCycle.EndDate?.ToString("dd MMM") ?? "Now";
               
                this.CropType = inCropCycle.CropTypeName.Substring(0, Math.Min(13, inCropCycle.CropTypeName.Length));
                if (inCropCycle.CropTypeName.Length > 13)
                    this.CropType += "..."; 

                CropVariety = inCropCycle.CropVariety.Substring(0, Math.Min(10, inCropCycle.CropVariety.Length));
                if (inCropCycle.CropVariety.Length > 10)
                    this.CropVariety += "...";

                LocationName = inCropCycle.Location.Name.Substring(0, Math.Min(10, inCropCycle.Location.Name.Length));
                if (inCropCycle.Location.Name.Length > 10)
                    this.LocationName += "...";


                Duration = Math.Round(((inCropCycle.EndDate ?? DateTimeOffset.Now) - inCropCycle.StartDate).TotalDays, 0).ToString() + " days";
            }
            public CropCycle CropCycle { get; set; }

            public string LocationName { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string CropType { get; set; }
            public string CropVariety { get; set; }
            public string Duration { get; set; }            
        }

        public override void Kill()
        {
            Messenger.Instance.TablesChanged.Unsubscribe(_updateAction);
        }
    }
}
