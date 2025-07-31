using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp9
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool manage;
        string path = "C:\\Users\\o.litvin\\Desktop\\Projekte\\WpfApp9\\saveDate.txt";
        private const string dbFilePath = "toDoDate.db";
        
        //SQLite
        public static void InitializeDB()
        {
            if (!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath);

                using var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;");
                connection.Open();

                string sqlCommand = "CREATE TABLE Tasks(" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT,  " +
                    "Done TEXT, " +
                    "Description TEXT, " +
                    "Category TEXT, " +
                    "StartDate TEXT, " +
                    "EndDate TEXT, " +
                    "Status TEXT" +
                    ")";

                using var command = new SQLiteCommand(sqlCommand, connection);
                command.ExecuteNonQuery();
            }
        }

        public void LoadTasksFromDatabase()
        {
            tasks.Clear();
            MyDataGrid.Items.Clear();

            using var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;");
            connection.Open();

            string sqlCommand = "SELECT Done, Description, Category, StartDate, EndDate, Status FROM Tasks";
            using var command = new SQLiteCommand(sqlCommand, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                char done = reader["Done"].ToString()[0];
                string description = reader["Description"].ToString();
                string category = reader["Category"].ToString();
                DateTime startDate = DateTime.Parse(reader["StartDate"].ToString());
                DateTime endDate = DateTime.Parse(reader["EndDate"].ToString());
                string status = reader["Status"].ToString();

                var task = new Task(done, description, category, startDate, endDate, status);

                tasks.Add(task);
                MyDataGrid.Items.Add(task);
            }
        }

        public void SaveTasksToDatabase()
        {
            using var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;");
            connection.Open();

            string sqlDeleteCommand = "DELETE FROM Tasks";
            using var deleteCommand = new SQLiteCommand(sqlDeleteCommand, connection);
            deleteCommand.ExecuteNonQuery();

            foreach(var task in tasks)
            {
                string sqlInsertCommand = "INSERT INTO Tasks (Done, Description, Category, StartDate, EndDate, Status)" +
                    "VALUES (@Done, @Description, @Category, @StartDate, @EndDate, @Status)";
                using var insertCommand = new SQLiteCommand(sqlInsertCommand, connection);

                insertCommand.Parameters.AddWithValue("@Done", task.Done.ToString());
                insertCommand.Parameters.AddWithValue("@Description", task.Description);
                insertCommand.Parameters.AddWithValue("@Category", task.Category);
                insertCommand.Parameters.AddWithValue("@StartDate", task.StartDate.ToString());
                insertCommand.Parameters.AddWithValue("@EndDate", task.EndDate.ToString());
                insertCommand.Parameters.AddWithValue("@Status", task.Status.ToString());

                insertCommand.ExecuteNonQuery();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            manage = false;

            //SQL
            InitializeDB();
            LoadTasksFromDatabase();

            //Set default time
            SaveStartDate.SelectedDate = DateTime.Today;
            SaveEndDate.SelectedDate = DateTime.Today.AddDays(1);

            //Load
            LoadCategory();
        }
        ObservableCollection<Task> tasks = new ObservableCollection<Task>
        {
            // Personal
            new Task('✓', "Go for a morning run", "Personal", DateTime.Today.AddDays(-2), DateTime.Today.AddDays(1), "Complete"),
            new Task('x', "Read a chapter of a book", "Personal", DateTime.Today, DateTime.Today.AddDays(2), "Pending"),
            new Task('✓', "Meditate for 10 minutes", "Personal", DateTime.Today.AddDays(2), DateTime.Today, "Complete"),

            // Shopping
            new Task('x', "Buy groceries", "Shopping", DateTime.Today, DateTime.Today.AddDays(1), "Pending"),
            new Task('x', "Order new headphones online", "Shopping", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), "Pending"),
            new Task('x', "Pick up prescription at pharmacy", "Shopping", DateTime.Today, DateTime.Today.AddDays(2), "Pending"),
            new Task('x', "Buy water", "Shopping", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), "Pending"),

            // Work
            new Task('✓', "Finish weekly report", "Work", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), "Complete"),
            new Task('x', "Attend team meeting", "Work", DateTime.Today, DateTime.Today, "Pending"),
            new Task('x', "Reply to client emails", "Work", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), "Pending"),

            // Errands
            new Task('x', "Drop off package at post office", "Errands", DateTime.Today, DateTime.Today.AddDays(1), "Pending"),
            new Task('x', "Get car washed", "Errands", DateTime.Today.AddDays(2), DateTime.Today.AddDays(2), "Pending"),
            new Task('x', "Pick up dry cleaning", "Errands", DateTime.Today.AddDays(1), DateTime.Today.AddDays(3), "Pending"),

            // Projects
            new Task('x', "Sketch wireframes for new app", "Projects", DateTime.Today.AddDays(-2), DateTime.Today.AddDays(5), "Pending"),
            new Task('✓', "Fix bugs in portfolio website", "Projects", DateTime.Today, DateTime.Today.AddDays(4), "Complete"),
            new Task('x', "Backup project files", "Projects", DateTime.Today, DateTime.Today.AddDays(1), "Pending")
        };

        public ObservableCollection<string> Category { get; set; } = new ObservableCollection<string>
        {
            "Personal", "Shopping", "Work", "Errands", "Projects"
        }; //Save using txt

        public void LoadCategory()
        {
            string loaded = File.ReadAllText("SaveCategory.txt");
            string[] splited = loaded.Split(";");

            Category.Clear();

            foreach(var item in splited)
            {
                Category.Add(item);
            }
        }

        private void CalendarInput_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalendarInput.SelectedDate != null)
            {
                var selectedDate = CalendarInput.SelectedDate.Value.Date;
                var filtered = tasks.Where(tasks => tasks.StartDate.Date == selectedDate).ToList();

                MyDataGrid.Items.Clear();

                foreach (var item in filtered)
                {
                    MyDataGrid.Items.Add(item);
                }
            }
        }

        private void All_Click(object sender, RoutedEventArgs e)
        {
            MyDataGrid.Items.Clear();

            foreach (var item in tasks)
            {
                MyDataGrid.Items.Add(item);
            }
        }

        private void Pending_Click(object sender, RoutedEventArgs e)
        {
            var pending = tasks.Where(tasks => tasks.Done == 'x').ToList();

            MyDataGrid.Items.Clear();

            foreach (var item in pending)
            {
                MyDataGrid.Items.Add(item);
            }
        }

        private void Complete_Click(object sender, RoutedEventArgs e)
        {
            var complete = tasks.Where(tasks => tasks.Done == '✓').ToList();

            MyDataGrid.Items.Clear();

            foreach (var item in complete)
            {
                MyDataGrid.Items.Add(item);
            }
        }

        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            ListBoxOutput.Items.Clear();

            foreach (var item in Category)
            {
                ListBoxOutput.Items.Add(item);
            }

            if (manage)
            {
                ManageCategoryPannel.Visibility = Visibility.Hidden;
                manage = false;
            }
            else
            {
                ManageCategoryPannel.Visibility = Visibility.Visible;
                manage = true;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string value = InputCategory.Text;
            int index = ListBoxOutput.SelectedIndex;

            if (index != -1)
            {
                string selected = ListBoxOutput.SelectedItem.ToString();
                var sorted = Category.Where(Category => Category != selected).ToList();
                Category.Clear();

                foreach(var item in sorted)
                {
                    Category.Add(item);
                }

                ListBoxOutput.Items.Clear();
                foreach(var item in Category)
                {
                    ListBoxOutput.Items.Add(item);
                }
                return;
        }

            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Error, empty input!");
                return;
            }
            else
            {
                bool checker = false;

                //Checker
                for (int i = 0; i < Category.Count; i++)
                {
                    if (value == Category[i])
                    {
                        checker = true;
                        break;
                    }
                }

                if (checker)
                {
                    var newList = Category.Where(Category => Category != value).ToList();
                    Category.Clear();

                    foreach (var item in newList)
                    {
                        Category.Add(item);
                    }
                }
                else
                {
                    Category.Add(value);
                }

                ListBoxOutput.Items.Clear();

                foreach (var item in Category)
                {
                    ListBoxOutput.Items.Add(item);
                }

                InputCategory.Text = "";
            }
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string category = button.Content.ToString();

                var filter = tasks.Where(tasks => tasks.Category == category).ToList();

                MyDataGrid.Items.Clear();

                foreach (var item in filter)
                {
                    MyDataGrid.Items.Add(item);
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {

            if(AddTaskPanel.Visibility == Visibility.Visible)
            {
                AddTaskPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                AddTaskPanel.Visibility = Visibility.Visible;
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            int selected = MyDataGrid.SelectedIndex;

            if (selected == -1)
            {
                MessageBox.Show("Item was not selected");
                return;
            }
            else
            {
                ObservableCollection<Task> newList = new ObservableCollection<Task>();

                for (int i = 0; i < tasks.Count; i++)
                {
                    if (i != selected)
                    {
                        newList.Add(tasks[i]);
                    }
                }
                tasks.Clear();
                MyDataGrid.Items.Clear();

                foreach (var item in newList)
                {
                    MyDataGrid.Items.Add(item);
                    tasks.Add(item);
                }
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string text = SaveText.Text;
            string category = SaveCombo.Text;
            DateTime startDate = SaveStartDate.SelectedDate.Value;
            DateTime endDate = SaveEndDate.SelectedDate.Value;

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show("Please fill all fields");
                return;
            }
            else
            {
                var push = new Task('x', text, category, startDate, endDate, "Pending");
                tasks.Add(push);

                MyDataGrid.Items.Clear();
                foreach(var item in tasks)
                {
                    MyDataGrid.Items.Add(item);
                }

                SaveCombo.Text = "";
                SaveText.Text = "";
                SaveStartDate.SelectedDate = DateTime.Today;
                SaveEndDate.SelectedDate = DateTime.Today.AddDays(1);
                AddTaskPanel.Visibility = Visibility.Hidden;
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string search = SearchBox.Text.Trim().ToLower();
            List<char> newList = new List<char>();
            string compare;

            if (string.IsNullOrWhiteSpace(search))
            {
                MyDataGrid.Items.Clear();
                foreach(var item in tasks)
                {
                    MyDataGrid.Items.Add(item);
                }
            }
            else //Search
            {
                var simple = tasks.Where(tasks => tasks.Description.ToLower() == search).ToList();

                if (simple.Count > 0)
                {
                    MyDataGrid.Items.Clear();
                    foreach(var item in simple)
                    {
                        MyDataGrid.Items.Add(simple);
                    }
                }
                else //first word, first letter
                {
                    for(int i = 0; i < search.Length; i++)
                    {
                        if (search[i] == ' ')
                        {
                            break;
                        }
                        else
                        {
                            newList.Add(search[i]);
                        }
                    }

                    search = string.Join("", newList);
                    compare = "-1";
                    int id = 0;
                    ObservableCollection<Task> display = new ObservableCollection<Task>();
                    //works

                    for (int i = 0; i < tasks.Count; i++)
                    {
                        string resultTask = tasks[i].Description;
                        List<char> searchList = new List<char>();

                        for(int j = 0; j < search.Length; j++)
                        {
                            if (resultTask[j] != ' ')
                            {
                                searchList.Add(resultTask[j]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        string convert = string.Join("", searchList).ToLower(); //problem

                        if (convert == search)
                        {
                            compare = convert;
                            display.Add(tasks[i]);
                        }
                    }

                    //Display
                    MyDataGrid.Items.Clear();
                    foreach(var item in display)
                    {
                        MyDataGrid.Items.Add(item);
                    }

                    //MessageBox.Show($"search: {search} compare: {compare}");
                }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SaveWindow.Visibility = Visibility.Visible;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        //Check Box
        private void MyDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            /*if(e.Column is DataGridCheckBoxColumn)
            {
                var task = e.Row.Item as Task;
                var checkbox = e.EditingElement as CheckBox;

                if(task != null && checkbox != null)
                {
                    bool isChecked = checkbox.IsChecked == true;

                    task.Status = isChecked ? "Complete" : "Pending";

                    MyDataGrid.Items.Refresh();
                }
            }*/
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                string content = button.Content.ToString();

                if(content == "✓")
                {
                    button.Content = 'x';
                    button.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    button.Content = '✓';
                }

                //Task name
                if(button.DataContext is Task task)
                {
                    string status = task.Status;
                    int id = 0;
                    ObservableCollection<Task> newList = new ObservableCollection<Task>();

                    //Find Id
                    for(int i = 0; i < tasks.Count; i++)
                    {
                        if (tasks[i].Description == task.Description)
                        {
                            id = i;
                            //MessageBox.Show($"Task: {tasks[i].Description} == {task.Description}, i = {i}");
                            break;
                        }
                    }

                    //Push before Id
                    for(int i = 0; i < id; i++)
                    {
                        newList.Add(tasks[i]);
                    }

                    //Change Status and Push
                    if(status == "Pending")
                    {
                        task.Status = "Complete";
                        task.Done = '✓';
                    }
                    else
                    {
                        task.Status = "Pending";
                        task.Done = 'x';
                        button.Foreground = new SolidColorBrush(Colors.Red);
                    }

                    newList.Add(task);
                    id+=1;

                    //Push Rest
                    if(id <= tasks.Count)
                    {
                        for(int i = id; i < tasks.Count; i++)
                        {
                            newList.Add(tasks[i]);
                        }
                    }

                    //Refresh DataGrid
                    tasks.Clear();
                    MyDataGrid.Items.Clear();

                    foreach(var item in newList)
                    {
                        tasks.Add(item);
                        MyDataGrid.Items.Add(item);
                    }

                    //MessageBox.Show($"newList: {newList.Count} Tasks: {tasks.Count}");

                    button.Background = new SolidColorBrush(Colors.Red);
                }
            }
        }

        private void SaveExit_Click(object sender, RoutedEventArgs e)
        {
            SaveTasksToDatabase();

            string saveFile = string.Join(";", Category);
            File.WriteAllText("SaveCategory.txt", saveFile);
            this.Close();
        }

        private void NoSaveExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SaveWindow.Visibility = Visibility.Hidden;
        }
    }

    public class Task
    {
        public char Done { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

        public Task() { }
        public Task(char done, string description, string category, DateTime startDate, DateTime endDate, string status)
        {
            Done = done;
            Description = description;
            Category = category;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
        }
    }
}