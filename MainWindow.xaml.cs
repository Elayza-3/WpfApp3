using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
   

    public partial class MainWindow : Window
    {
        
        private string[] files;
        private List<string> listening_files = new List<string>();
        private bool isProgram = false;
        private bool play = false;
        private bool retry = false;
        private bool drag = false;
        private bool random = false;
        private Thread thread;

        public MainWindow()
        {
            InitializeComponent();

        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media.Volume = volume.Value;

        }

        private void load_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if(result == CommonFileDialogResult.Ok)
            {
                files = Directory.GetFiles(dialog.FileName,"*.mp3");

                foreach (var item in files)
                {
                    tracks.Items.Add(Path.GetFileNameWithoutExtension(item));
                }

                tracks.SelectedIndex = 0;
                                
            }
        }


        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            duration.Maximum = media.NaturalDuration.TimeSpan.Ticks;
            timer_end.Text = media.NaturalDuration.TimeSpan.ToString("hh\\:mm\\:ss");

            listening_files.Add(tracks.SelectedItem.ToString());

            Time();
        }


        

        private void duration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if ( (!drag && !isProgram) && e.NewValue == duration.Maximum)
            {
                NextTrack(retry);
                return;

            }

        }

        private void Start(string filename)
        {
            isProgram = true;
            duration.Value = 0;
            isProgram = false;


            media.Close();

            media.Source = new Uri(filename, UriKind.RelativeOrAbsolute);
            media.Volume = volume.Value;
            timer_start.Text = "00:00:00";
            media.Pause();


        }

        private void duration_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            drag = true;
        }

        private void duration_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            drag = false;

            timer_start.Text = new TimeSpan(Convert.ToInt64(duration.Value)).ToString("hh\\:mm\\:ss");
            media.Position = new TimeSpan(Convert.ToInt64(duration.Value));
           
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            play = !play;

            if (play)
            {
                thread.Abort();media.Pause();
            }
            else
            {
                Time();
            }
        }

        private void forward_Click(object sender, RoutedEventArgs e)
        {
            if ((tracks.SelectedIndex + 1) == files.Length)
            {
                tracks.SelectedIndex = 0;
            }

            else tracks.SelectedIndex += 1;

            if (thread.IsAlive) thread.Abort();
        }

        private void backward_Click(object sender, RoutedEventArgs e)
        {

            if (tracks.SelectedIndex == 0)
            {
                tracks.SelectedIndex = files.Length - 1;
            }
            else tracks.SelectedIndex -= 1;

            if (thread.IsAlive) thread.Abort();
        }

        private void listening_Click(object sender, RoutedEventArgs e)
        {
            NewWindow window = new NewWindow(listening_files);
            window.ShowDialog();

            int ind;

            if (window.track != null && (ind = tracks.Items.IndexOf(window.track)) != -1) tracks.SelectedIndex = ind;
        }
        private void Time()
        {
            long Value = TimeSpan.Parse(timer_start.Text).Ticks;
            long Ticks = media.NaturalDuration.TimeSpan.Ticks;


            thread = new Thread(_ =>
            {

                while (Value < Ticks)
                {
                    Thread.Sleep(1000);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Value = media.Position.Ticks + (new TimeSpan(0, 0, 0, 1)).Ticks;

                        timer_start.Text = (new TimeSpan(Value)).ToString("hh\\:mm\\:ss");
                        if (!drag)
                        {isProgram = true; duration.Value = Value; isProgram = false;
                        }
                    });


                }

                Application.Current.Dispatcher.Invoke(() => NextTrack(retry));

            }
            );
            thread.IsBackground = true;

            media.Play();
            thread.Start();

        }

        private void NextTrack(bool repeate = false)
        {
            if (thread.IsAlive) thread.Abort();

            if(repeate)
            {
                Start(files[tracks.SelectedIndex]);
                return;
            }
            if ((tracks.SelectedIndex + 1) == files.Length)
            {
                tracks.SelectedIndex = 0;
            }

            else tracks.SelectedIndex += 1;

            

        }

        private void rand_Click(object sender, RoutedEventArgs e)
        {
            random = !random;
            Random rand = new Random();
            if (random)
            {

                for (int i = files.Length - 1; i > 0; i--)
                {
                    int j = rand.Next(i + 1);
                    string temp = files[i];
                    files[i] = files[j];
                    files[j] = temp;
                }
                
            }
            else Array.Sort(files);
            tracks.Items.Clear();

            foreach (var item in files)
            {
                tracks.Items.Add(Path.GetFileNameWithoutExtension(item));
            }
            tracks.Items.Refresh();
            tracks.SelectedIndex = 0;


        }

        private void again_Click(object sender, RoutedEventArgs e)
        {
            retry = !retry;

        }

        private void tracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (thread != null && thread.IsAlive) thread.Abort(); Start(files[tracks.SelectedIndex]); 
            }
            catch { }
            
        }

        
    }
}
