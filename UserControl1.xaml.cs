using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace CreateOptStructures
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        private StructureSet _ss;
        private Structure _ptvsSum;
        private Double _marginToCrop;

        public UserControl1(StructureSet ss)
        {
            InitializeComponent();
            _ss = ss;
            _ptvsSum = CreatePtvsSum();
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

        private void marginToCropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            oarsStackPanel.Children.Clear();

            _marginToCrop = Convert.ToDouble(((ComboBoxItem)marginToCropComboBox.SelectedItem).Content.ToString());

            foreach (var oar in _ss.Structures.Where(x => x.DicomType == "ORGAN").ToList()) // aqui NÃO funciona sem ".ToList()
            {
                if (ThereIsOverlap(oar, _ptvsSum, _marginToCrop))                    
                    oarsStackPanel.Children.Add(new CheckBox() { Content = oar.Id, IsChecked = true });
            }

            if (oarsStackPanel.Children.Count > 0)
                gerarEstruturasButton.IsEnabled = true;
            else
            {
                gerarEstruturasButton.IsEnabled = false;
                MessageBox.Show("Não há intersecção entre 'OAR' e 'PTV' para a margem selecionada.", "CreateOpStructure", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in oarsStackPanel.Children)
            {
                if ((bool)cb.IsChecked)
                {
                    Structure oar = _ss.Structures.FirstOrDefault(x => x.Id.ToLower() == cb.Content.ToString().ToLower());

                    string id = oar.Id + "_opt";

                    if (_ss.CanAddStructure("CONTROL", id))
                        _ss.AddStructure("CONTROL", id);

                    Structure structureOpt = _ss.Structures.FirstOrDefault(x => x.Id.ToLower() == id.ToLower());
                    structureOpt.SegmentVolume = oar.Sub(_ptvsSum.Margin(_marginToCrop));

                    MessageBox.Show(string.Format($"Estrutura {id} gerada."));
                }
            }
        }
    }
}
