using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPICreateDuct
{
    public class MVVM
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; set; } = new List<DuctType>();
        public List<Level> Levels { get; set; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double DuctOffset { get; set; }
        public List<XYZ> Points { get; set; } = new List<XYZ>();
        public DuctType SelectedDuctType { get; set; }
        public Level SelectedLevel { get; set; }
        public MEPSystemType DuctSystemType { get; set; }

        public MVVM(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctsUtils.GetDuctTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            DuctOffset = 2500;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
            DuctSystemType = DuctsUtils.GetDuctSystemTypes(commandData);
        }


        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || SelectedDuctType == null || SelectedLevel == null)
                return;

            XYZ firstPoint = null;
            XYZ secondPoint = null;
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;
                firstPoint = Points[i - 1];
                secondPoint = Points[i];
            }

            using (Transaction ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();

                Duct duct = Duct.Create(doc, DuctSystemType.Id, SelectedDuctType.Id, SelectedLevel.Id, firstPoint, secondPoint);
                Parameter parameter = duct.get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM);
                parameter.Set(UnitUtils.ConvertToInternalUnits(DuctOffset, UnitTypeId.Millimeters));
                ts.Commit();
            }

            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

    }
}
