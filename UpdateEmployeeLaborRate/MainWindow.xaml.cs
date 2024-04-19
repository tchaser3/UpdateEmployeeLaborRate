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
using DepartmentDLL;
using EmployeeLaborRateDLL;
using NewEmployeeDLL;
using NewEventLogDLL;
using DataValidationDLL;

namespace UpdateEmployeeLaborRate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        DepartmentClass TheDepartmentClass = new DepartmentClass();
        EmployeeLaborRateClass TheEmployeeLaborRateClass = new EmployeeLaborRateClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();

        FindSortedDepartmentDataSet TheFindSortedDepartmentDataSet = new FindSortedDepartmentDataSet();
        FindEmployeeByDepartmentDataSet TheFindEmployeeByDepartmentDataSet = new FindEmployeeByDepartmentDataSet();

        string gstrDepartment;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;

            cboSelectDepartment.Items.Add("Select Department");

            TheFindSortedDepartmentDataSet = TheDepartmentClass.FindSortedDepartment();

            intNumberOfRecords = TheFindSortedDepartmentDataSet.FindSortedDepartment.Rows.Count - 1;

            for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
            {
                cboSelectDepartment.Items.Add(TheFindSortedDepartmentDataSet.FindSortedDepartment[intCounter].Department);
            }

            cboSelectDepartment.SelectedIndex = 0;
        }

        private void cboSelectDepartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int intSelectedIndex;

            intSelectedIndex = cboSelectDepartment.SelectedIndex - 1;

            if (intSelectedIndex > -1)
                gstrDepartment = TheFindSortedDepartmentDataSet.FindSortedDepartment[intSelectedIndex].Department;

            TheFindEmployeeByDepartmentDataSet = TheEmployeeClass.FindEmployeeByDepartment(gstrDepartment);

            dgrResults.ItemsSource = TheFindEmployeeByDepartmentDataSet.FindEmployeeByDepartment;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //setting up local variables
            string strErrorMessage = "";
            bool blnFatalError = false;
            bool blnThereIsAProblem = false;
            string strValueForValdation;
            decimal decLaborRate = 0;
            int intEmployeeID;
            int intCounter;
            int intNumberOfRecords;

            try
            {
                if(cboSelectDepartment.SelectedIndex < 1)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Department Was Not Chosen\n";
                }
                strValueForValdation = txtLaborRate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDoubleData(strValueForValdation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Labor Rate is not a number\n";
                }
                else
                {
                    decLaborRate = Convert.ToDecimal(strValueForValdation);
                }
                if(blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage(strErrorMessage);
                    return;
                }

                intNumberOfRecords = TheFindEmployeeByDepartmentDataSet.FindEmployeeByDepartment.Rows.Count - 1;

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    intEmployeeID = TheFindEmployeeByDepartmentDataSet.FindEmployeeByDepartment[intCounter].EmployeeID;

                    blnFatalError = TheEmployeeLaborRateClass.UpdateEmployeeLaborRate(intEmployeeID, decLaborRate);

                    if (blnFatalError == true)
                        throw new Exception();
                }

                TheMessagesClass.InformationMessage("Routine Complete");

                txtLaborRate.Text = "";
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Update Employee Labor Rate // Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
    }
}
