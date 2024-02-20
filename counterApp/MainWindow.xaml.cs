using System.Configuration;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System;
using System.Printing;

namespace counterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Boolean DataExported = false;
        private Boolean fileExists = false;

        private DateTime oldDate = new DateTime();


        public MainWindow()
        {
            InitializeComponent();
            oldDate = DateExists();
        }

        private void IncrementDigital(object sender, RoutedEventArgs e)
        {
            int DigitalCounter = Convert.ToInt32(DigitalCount.Text);
            DigitalCount.Text = $"{DigitalCounter + 1}";
            DataExported = false;
        }
        private void IncrementDigitalError(object sender, RoutedEventArgs e)
        {
            int DigitalErrorCounter = Convert.ToInt32(DigitalErrorCount.Text);
            DigitalErrorCount.Text = $"{DigitalErrorCounter + 1}";
            DataExported = false;
        }

        private void IncrementFixed(object sender, RoutedEventArgs e)
        {
            int FixedCounter = Convert.ToInt32(FixedCount.Text);
            FixedCount.Text = $"{FixedCounter + 1}";
            DataExported = false;
        }
        private void IncrementFixedError(object sender, RoutedEventArgs e)
        {
            int FixedErrorCounter = Convert.ToInt32(FixedErrorCount.Text);
            FixedErrorCount.Text = $"{FixedErrorCounter + 1}";
            DataExported = false;
        }

        private void IncrementRemovable(object sender, RoutedEventArgs e)
        {
            int RemovableCounter = Convert.ToInt32(RemovableCount.Text);
            RemovableCount.Text = $"{RemovableCounter + 1}";
            DataExported = false;
        }
        private void IncrementRemovableError(object sender, RoutedEventArgs e)
        {
            int RemovableErrorCounter = Convert.ToInt32(RemovableErrorCount.Text);
            RemovableErrorCount.Text = $"{RemovableErrorCounter + 1}";
            DataExported = false;
        }

        private void InfoPopup(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This tool is used to keep track of all cases entered of a given type throughout the day. If you make any errors, you can click the smaller \"+1 Error\" buttons to add an error for that category. \n\nAt the end of the day, when you are finished entering cases, you can click the Export button to save your case counts as a .CSV file (which is openable with Microsoft Excel). This lets you keep track of how many cases you enter each day without needing to keep sticky notes, and you can go back and edit it any time!\n\n(Exported CSV files will be stored inside a folder called CaseTracking, where this program is located) \n\nKeep in mind that if you are notified of an error from a previous day you will need to manually select that date to edit the error count - by default this program only tracks the current day's errors.\n\nThanks for using Bo's case tracker!");
        }

        private void Reset()
        {
            DigitalCount.Text = "0";
            DigitalErrorCount.Text = "0";
            RemovableCount.Text = "0";
            RemovableErrorCount.Text = "0";
            FixedCount.Text = "0";
            FixedErrorCount.Text = "0";
            DataExported = false;
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            Reset();
        }


        private Boolean dateReturned = false;

        private void SetDatePicker()
        {
            if (EditDate != null) {
                EditDate.SelectedDateChanged -= DateChange;
                dateReturned = true;
                EditDate.SelectedDate = oldDate.Date;
                EditDate.SelectedDateChanged += DateChange;
            }
        }
        private void SetDatePicker(DateTime d)
        {
            if (EditDate != null)
            {
                EditDate.SelectedDateChanged -= DateChange;
                dateReturned = true;
                EditDate.SelectedDate = d;
                EditDate.SelectedDateChanged += DateChange;
            }
        }

        private Boolean IsDatePickerStillDefault()
        {
            if (EditDate.SelectedDate == null || EditDate.SelectedDate == DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }
        private void DateChange(object? sender, RoutedEventArgs e)
        {
            Boolean defaultDate = IsDatePickerStillDefault();
            if (!DataExported && !dateReturned && !defaultDate)
            {
                MessageBoxResult answer = MessageBox.Show("***Warning!***\n\nYou haven't yet exported the data for the date that is currently selected. Please make sure you export first. Click yes to change date anyway, or click no to go back.", "Export Warning!", MessageBoxButton.YesNo);
                if (answer == MessageBoxResult.No)
                {
                    DataExported = false;
                    dateReturned = true;
                    SetDatePicker();
                    return;
                }
            }
            DateTime dateTime = DateExists();
            FileCheck(GetFile(dateTime));
            if (fileExists)
            {
                if (DateExistsInFile(dateTime) >= 0)
                {
                    MessageBox.Show("This date exists in the file! Loading data.");
                    string[] data = GetData(dateTime);
                    if (data.Length < 7)
                    {
                        MessageBox.Show("Unfortunately, the data from the file for this specific date seems to be corrupted. Please manually edit if necessary - there is not enough information to import successfully.");
                    }
                    DigitalCount.Text = data[1];
                    DigitalErrorCount.Text = data[2];
                    FixedCount.Text = data[3];
                    FixedErrorCount.Text = data[4];
                    RemovableCount.Text = data[5];
                    RemovableErrorCount.Text = data[6];
                    DataExported = false;
                }
                else
                {
                    Reset();
                    DataExported = false;

                }
                oldDate = dateTime;
                dateReturned = false;
            }
            return;
        }

        private string[] GetData()
        {
            return GetData(DateExists());
        }

        private string[] GetData(DateTime date)
        {
            string path = GetFile(date);


            //var headerLine = "DATE,DIGITAL CASES,DIGITAL ERRORS,TRADITIONAL FIXED CASES,TRADITIONAL FIXED ERRORS,REMOVABLE CASES,REMOVABLE ERRORS,TOTAL CASES ENTERED,TOTAL ERRORS,ERROR PERCENT";
            //var caseLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", editDay, DigitalCases, DigitalErrors, FixedCases, FixedErrors, RemovableCases, RemovableErrors, TotalCasesEntered.ToString(), TotalErrors.ToString(), ErrorPercent.ToString());
            int index = DateExistsInFile(date);
            string[] linesInFile = GetFileAsArray();
            string[] editLine = linesInFile[index].Split(",");
            return editLine;

        }

        private DateTime DateExists()
        {
            DateTime d;
            if (EditDate.SelectedDate != null)
            {
                d = (DateTime)EditDate.SelectedDate;
            }
            else
            {
                d = DateTime.UtcNow;
                SetDatePicker(d);
            }
            return d;
        }

        private string GetFile()
        {
            return GetFile(DateExists());

            //DateTime dateTime = DateExists();
            //var editMonth = dateTime.Month.ToString() + "-" + dateTime.Year.ToString();

            //string filePath = @".\CaseTracking\" + editMonth + ".csv";
            //return filePath;
        }

        private string GetFile(DateTime date)
        {
            var editMonth = date.Month.ToString() + "-" + date.Year.ToString();

            string filePath = @".\CaseTracking\" + editMonth + ".csv";
            return filePath;
        }

        private string[] GetFileAsArray()
        {
            FileCheck(GetFile());
            string path = GetFile();
            //file is not locked, continue

            // read all the lines in the file and move info to a string array

            string[] linesInFile = File.ReadAllLines(@path);
            return linesInFile;
        }

        private string[] GetFileAsArray(string path)
        {
            string[] linesInFile = new string[0];
            if (NoFileError(FileCheck(GetFile())))
            {
                linesInFile = File.ReadAllLines(@path);
                return linesInFile;
            }
            else
            {
                return linesInFile;
            }
        }


        // FileCheck determines whether the file exists, and if the file is readable.
        // If the file exists and is not readable, it will display a message box to the user
        // warning them to close the file so the program may continue.
        private string FileCheck(string path)
        {
            return FileCheck(path, 0);
        }

        private string FileCheck(string path, int b)
        {
            DateTime dateTime = DateExists();

            //path = @".\CaseTracking\" + editMonth + ".csv";

            //string[] linesInFile = GetFileAsArray(@path);

            if (!System.IO.Directory.Exists(@".\CaseTracking\"))
            {
                System.IO.Directory.CreateDirectory(@".\CaseTracking\");
            }
            if (System.IO.File.Exists(@path))
            {
                fileExists = true;
                try
                {
                    FileInfo f = new FileInfo(@path);
                    using (var stream = f.Open(FileMode.Open, FileAccess.ReadWrite))
                    {
                        StreamReader r = new StreamReader(stream);
                        r.ReadToEnd();
                    }
                    return @path;
                }
                catch (IOException e)
                {
                    System.Console.Write(e.ToString());
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    if (fileExists)
                    {
                        MessageBox.Show("There was an error while trying to read the file. Please ensure that the file is closed in any other programs, then continue. \n\nNote that this may occur when changing the date when the file does not yet exist.");
                    }
                    return "Error!";
                }
            }
            else
            {
                try
                {
                    //File.Create(@path);
                    File.WriteAllText(@path, "");
                    return "Error!";
                } catch (IOException e)
                {
                    MessageBox.Show("There was an error exporting. Please try again.");
                }
            }
            return "Error!";
        }


        private Boolean NoFileError(string temp)
        {
            if (temp == "Error!" && fileExists)
            {
                MessageBox.Show("The directory and/or file did not exist in the location of the program, or was otherwise inaccessible. Please ensure that you have not moved the file or folder where information is saved. \n\nThe folder has been created if it was not already present. Please verify the location of your files, then try to export again.\n\nThis error may show the first time you attempt to export data. Please wait a moment and try again if this is the case.");
                return false;
            }
            if (!fileExists)
            {
                return false;
            }
            return true;
        }

        private int DateExistsInFile(DateTime dateTime)
        {
            FileCheck(GetFile(dateTime));
            string path = GetFile(dateTime);

            
            var editDay = dateTime.Month.ToString() + "/" + dateTime.Day.ToString() + "/" + dateTime.Year.ToString();
            //file is not locked, continue

            // read all the lines in the file and move info to a string array
            string[] linesInFile = GetFileAsArray(@path);
                
            int index = 0;
            Boolean dayExists = false;
            foreach (string line in linesInFile)
            {
                string lineDate = line.Split(",")[0];
                if (lineDate == editDay)
                {
                    dayExists = true;
                    break;
                }
                index++;
            }

            if (dayExists)
            {
                return index;
            }
            
            return -1;
        }

        private void CleanupFile(string path)
        {
            string[] linesInFile = File.ReadAllLines(@path);
            var ordered = linesInFile.Skip(1).OrderBy(line => DateTime.Parse(line.Split(",")[0])).ToList();

            int sortIndex = 1;
            foreach (var line in ordered)
            {
                linesInFile[sortIndex] = line;
                sortIndex++;
            }

            File.WriteAllLines(@path, linesInFile);
        }

        private void ExportData(object sender, RoutedEventArgs e)
        {
            /* We want to be able to store and edit data for any given day of the month.
             * We will have a calendar to pick the day that is being saved/changed.
             */
            // Name / Date / Digital # / Digital Error / Traditional # / Traditional Error / Removable # / Removable Error / Total Cases / Errors / Percent

            DateTime dateTime = DateExists();


            // var editMonth = dateTime.Month.ToString() + "-" + dateTime.Year.ToString();
            var editDay = dateTime.Month.ToString() + "/" + dateTime.Day.ToString() + "/" + dateTime.Year.ToString();
            // String filePath = @".\CaseTracking\" + dateTime.Month.ToString() + "-" + dateTime.Day.ToString() + "-" + dateTime.Year.ToString() + ".csv";

            string filePath = GetFile(dateTime);
    

            var DigitalCases = DigitalCount.Text;
            var DigitalErrors = DigitalErrorCount.Text;
            var RemovableCases = RemovableCount.Text;
            var RemovableErrors = RemovableErrorCount.Text;
            var FixedCases = FixedCount.Text;
            var FixedErrors = FixedErrorCount.Text;
            int TotalCasesEntered = (Int32.Parse(DigitalCases) + Int32.Parse(FixedCases) + Int32.Parse(RemovableCases));
            //int TotalCasesEntered = Int32.Parse(DigitalCases + FixedCases + RemovableCases);
            int TotalErrors = (Int32.Parse(DigitalErrors) + Int32.Parse(FixedErrors) + Int32.Parse(RemovableErrors));
            //int TotalErrors = Int32.Parse(DigitalErrors + FixedErrors + RemovableErrors);
            double ErrorPercent = 0;
            if (TotalCasesEntered != 0)
            {
                ErrorPercent = (double)TotalErrors / TotalCasesEntered;
                ErrorPercent *= 100;
                ErrorPercent = Math.Round(ErrorPercent, 2);
            }
            else
            {
                MessageBox.Show("You did not enter any cases, and the program will not be able to save your data. Please try again.");
                return;
            }
            var headerLine = "DATE,DIGITAL CASES,DIGITAL ERRORS,TRADITIONAL FIXED CASES,TRADITIONAL FIXED ERRORS,REMOVABLE CASES,REMOVABLE ERRORS,TOTAL CASES ENTERED,TOTAL ERRORS,ERROR PERCENT";
            var caseLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", editDay, DigitalCases, DigitalErrors, FixedCases, FixedErrors, RemovableCases, RemovableErrors, TotalCasesEntered.ToString(), TotalErrors.ToString(), ErrorPercent.ToString());


            // var digitalLine = string.Format("{0},{1}", DigitalCases, DigitalErrors);
            // var removableLine = string.Format("{0},{1}", RemovableCases, RemovableErrors);
            // var fixedLine = string.Format("{0},{1}", FixedCases, FixedErrors);

            string temp = FileCheck(@filePath);
            if (!NoFileError(temp))
            {
                goto ErrorState;
            }

            string[] linesInFile = GetFileAsArray();

            if (linesInFile.Length <= 0) 
            {
                MessageBox.Show("It appears that the file you are trying to load or save to is empty. Click OK to attempt to overwrite the file.");
                goto EmptyFile;
            }

            
            

            int dateIndex = DateExistsInFile(dateTime);
            
            if (System.IO.File.Exists(@filePath))
            {
                Boolean dayExists = false;
                FileCheck(@filePath);
                //file is not locked, continue
                
                // read all the lines in the file and move info to a string array
                //linesInFile = File.ReadAllLines(@filePath);
                // after reading the file, edit the necessary line
                if (dateIndex >= 0)
                {
                    dayExists = true;
                    linesInFile[dateIndex] = caseLine;
                }
                else
                {
                    int index = 0;
                    foreach (string line in linesInFile)
                    {
                        string currentLine = line.Split(",")[0];
                        if (currentLine == editDay)
                        {
                            linesInFile[index] = caseLine;
                            dayExists = true;
                            break;
                        }
                        index++;
                    }
                }
                //Array.Sort(linesInFile,1,linesInFile.Length-1);

                //var ordered = linesInFile.OrderBy(line => line.Split(",")[0]).ToList();

                var ordered = linesInFile.Skip(1).OrderBy(line => DateTime.Parse(line.Split(",")[0])).ToList();

                int sortIndex = 1;
                foreach (var line in ordered)
                {
                    linesInFile[sortIndex] = line;
                    sortIndex++;
                }

                if (dayExists)
                {
                    File.WriteAllLines(@filePath, linesInFile);
                    DataExported = true;
                    MessageBox.Show("Exported successfully! Updated information for " + editDay);
                    return;
                }
                File.WriteAllLines(@filePath, linesInFile);
                if (!dayExists)
                {
                    
                    //File.AppendAllLines(@filePath, Environment.NewLine + caseLine);
                    File.AppendAllText(@filePath, caseLine);
                    CleanupFile(@filePath);
                    DataExported = true;
                    MessageBox.Show("Exported successfully! Added data to a new line.");
                    return;
                }
            }
            EmptyFile:
            // if the file doesn't exist, write all info to new file using WriteAllText
            File.WriteAllText(@filePath, headerLine + "\n" + caseLine);

            
            DataExported = true;
            MessageBox.Show("Exported successfully! A file was created, or an existing file has been overwritten.");
            return;
            ErrorState:
            MessageBox.Show("There was an error while exporting. This may occur if the file has not yet been created. Please try to export your data again.");
            return;
        }

    }
}