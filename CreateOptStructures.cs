using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: uncomment the line below if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {

        }

        private StructureSet _ss;

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here your code that is called when the script is launched from Eclipse

            _ss = context.StructureSet;

            if (_ss == null)
            {
                MessageBox.Show("Carregue um paciente e um conjunto de estruturas antes de executar o script");
                return;
            }

            context.Patient.BeginModifications();

            var ptvsSum = CreatePtvsSum();

            if (ptvsSum == null)
            {
                MessageBox.Show("Estrutura 'PTV' não encontrada.");
                return;
            }

            foreach (var structure in _ss.Structures.Where(s => s.DicomType == "ORGAN").ToList())
            {
                if (ThereIsOverlap(structure, ptvsSum))
                {
                    if (_ss.CanAddStructure("CONTROL", structure.Id + "_opt"))
                        _ss.AddStructure("CONTROL", structure.Id + "_opt");

                    _ss.Structures.FirstOrDefault(s => s.Id.ToLower() == structure.Id.ToLower() + "_opt").SegmentVolume = structure.Sub(ptvsSum.Margin(2));
                }                
            }           
        }

        private Structure CreatePtvsSum()
        {
            List<Structure> ptvs = _ss.Structures.Where(s => s.DicomType == "PTV").ToList();

            if (_ss.CanAddStructure("CONTROL", "ptvs_sum"))
                _ss.AddStructure("CONTROL", "ptvs_sum");

            var ptvsSum = _ss.Structures.FirstOrDefault(s => s.Id.ToLower() == "ptvs_sum");

            ptvsSum.SegmentVolume = ptvs[0].SegmentVolume;
            for (var i = 1; i < ptvs.Count(); i++)
                ptvsSum.SegmentVolume = ptvsSum.Or(ptvs[i]);

            return ptvsSum;
        }

        private bool ThereIsOverlap(Structure st1, Structure st2, double margin = 2)
        {
            bool result = false;

            if (_ss.CanAddStructure("CONTROL", "temp"))
                _ss.AddStructure("CONTROL", "temp");

            var temp = _ss.Structures.FirstOrDefault(s => s.Id.ToLower() == "temp");

            temp.SegmentVolume = st1.And(st2.Margin(margin));

            if (!temp.IsEmpty)
                result = true;

            if (_ss.CanRemoveStructure(temp))
                _ss.RemoveStructure(temp);

            return result;
        }
    }
}
