using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using CreateOptStructures;


// TODO: uncomment the line below if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {

        }        

        public void Execute(ScriptContext context , System.Windows.Window window/*, ScriptEnvironment environment*/)
        {
            // TODO : Add here your code that is called when the script is launched from Eclipse

            var ss = context.StructureSet;

            if (ss == null)
            {
                MessageBox.Show("Carregue um paciente e um conjunto de estruturas antes de executar o script");
                return;
            }

            context.Patient.BeginModifications();

            var userControl = new UserControl1(context.StructureSet);
            window.Height = 540;
            window.Width = 375;
            window.Title = "Gerador de estruturas de otimização";
            window.Content = userControl;          
        }        
    }
}
