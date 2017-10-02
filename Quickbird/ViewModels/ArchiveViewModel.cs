namespace Quickbird.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using DbStructure.User;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Util;

    public class ArchiveViewModel : ViewModelBase
    {
        private CropCyclePresenter _selectedCropCycle;
        private readonly Action<string> _updateAction;

        public ArchiveViewModel()
        {
            _updateAction = Update;
            BroadcasterService.Instance.TablesChanged.Subscribe(_updateAction);
            Update(string.Empty);
        }

        public List<int> craps { get; set; } = new List<int>(new[] {1, 2, 3, 4});

        public ObservableCollection<CropCyclePresenter> CropCycles { get; set; } =
            new ObservableCollection<CropCyclePresenter>();

        public string CropType { get { return _selectedCropCycle?.CropCycle.CropTypeName ?? " --- "; } }

        public string Duration
        {
            get
            {
                if (_selectedCropCycle == null)
                    return " --- ";
                var result = (_selectedCropCycle.CropCycle.EndDate ?? DateTimeOffset.Now) -
                             _selectedCropCycle.CropCycle.StartDate;
                return Math.Round(result.TotalDays, 1) + " days";
            }
        }

        public string EndDate
        {
            get
            {
                if (_selectedCropCycle == null)
                    return " --- ";
                return _selectedCropCycle.CropCycle.EndDate?.ToString("dd MMM yyyy") ?? "On Going";
            }
        }

        public object SelectedCropCycle
        {
            get { return _selectedCropCycle; }
            set
            {
                _selectedCropCycle = (CropCyclePresenter) value;
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
            get
            {
                if (_selectedCropCycle == null)
                    return " --- ";
                return _selectedCropCycle.CropCycle.StartDate.ToString("dd MMM yyyy");
            }
        }

        public string Variety { get { return _selectedCropCycle?.CropCycle.CropVariety ?? " --- "; } }

        public string Yield
        {
            get
            {
                if (_selectedCropCycle == null)
                    return " --- ";
                return _selectedCropCycle.CropCycle.Yield + " kg";
            }
        }

        public override void Kill() { BroadcasterService.Instance.TablesChanged.Unsubscribe(_updateAction); }

        public async void Update(string input)
        {
            using (var db = new MainDbContext())
            {
                var cropCycles =
                    await
                        db.CropCycles.Where(cc => cc.EndDate != null && cc.EndDate < DateTimeOffset.Now)
                            .Include(cc => cc.Location)
                            .ToListAsync();

                for (var i = 0; i < cropCycles.Count; i++)
                {
                    var matchFound = false;
                    for (var m = 0; m < CropCycles.Count; m++)
                    {
                        if (cropCycles[i].ID == CropCycles[m].CropCycle.ID)
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
        }

        public class CropCyclePresenter
        {
            public CropCyclePresenter(CropCycle inCropCycle)
            {
                CropCycle = inCropCycle;
                StartDate = inCropCycle.StartDate.ToString("dd MMM");
                EndDate = inCropCycle.EndDate?.ToString("dd MMM") ?? "Now";

                CropType = inCropCycle.CropTypeName.Substring(0, Math.Min(13, inCropCycle.CropTypeName.Length));
                if (inCropCycle.CropTypeName.Length > 13)
                    CropType += "...";

                CropVariety = inCropCycle.CropVariety.Substring(0, Math.Min(10, inCropCycle.CropVariety.Length));
                if (inCropCycle.CropVariety.Length > 10)
                    CropVariety += "...";

                LocationName = inCropCycle.Location.Name.Substring(0, Math.Min(10, inCropCycle.Location.Name.Length));
                if (inCropCycle.Location.Name.Length > 10)
                    LocationName += "...";


                Duration =
                    Math.Round(((inCropCycle.EndDate ?? DateTimeOffset.Now) - inCropCycle.StartDate).TotalDays, 0) +
                    " days";
            }

            public CropCycle CropCycle { get; set; }
            public string CropType { get; set; }
            public string CropVariety { get; set; }
            public string Duration { get; set; }
            public string EndDate { get; set; }

            public string LocationName { get; set; }
            public string StartDate { get; set; }
        }
    }
}
