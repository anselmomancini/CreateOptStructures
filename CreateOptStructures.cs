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

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here your code that is called when the script is launched from Eclipse

            StructureSet ss = context.StructureSet;

            if (ss == null)
            {
                MessageBox.Show("Carregue um paciente e um conjunto de estruturas antes de executar o script");
                return;
            }

            context.Patient.BeginModifications();

            #region encontrar ptv usando foreach e declarando a variável como Structure
            //Structure ptv = null;
            //foreach (Structure structure in _ss.Structures)
            //{
            //    if (structure.DicomType == "PTV")
            //    {
            //        ptv = structure;
            //        break;
            //    }
            //}
            #endregion

            var ptv = ss.Structures.FirstOrDefault(s => s.DicomType == "PTV");

            if (ptv == null)
            {
                MessageBox.Show("Estrutura 'PTV' não encontrada.");
                return;
            }

            var reto = ss.Structures.FirstOrDefault(s => s.Id.ToLower() == "reto");
            if (reto == null)
                MessageBox.Show("Estrutura 'Reto' não encontrada.");
            else
            {
                // fazer sem checar se pode ser adicionada e criando a Structure retoOpt. Na sequência, explicar a necessidade de verificar (fazendo como está na bexiga)
                Structure retoOpt = ss.AddStructure("CONTROL", "reto_opt");
                retoOpt.SegmentVolume = reto.Sub(ptv);
            }

            var bexiga = ss.Structures.FirstOrDefault(s => s.Id.ToLower() == "bexiga");
            if (bexiga == null)
                MessageBox.Show("Estrutura 'Bexiga'não encontrada.");
            else
            {
                if (ss.CanAddStructure("CONTROL", "bexiga_opt"))
                    ss.AddStructure("CONTROL", "bexiga_opt");

                Structure beaxigaOpt = ss.Structures.FirstOrDefault(s => s.Id.ToLower() == "bexiga_opt");
                beaxigaOpt.SegmentVolume = bexiga.Sub(ptv);

                // mostrar a possibilidade de:
                //ss.Structures.FirstOrDefault(s => s.Id.ToLower() == "bexiga_opt").SegmentVolume = bexiga.Sub(ptv);
            }
        }
    }
}
