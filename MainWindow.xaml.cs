using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace SumarizandoVideo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void AddVideo_Click(object sender, RoutedEventArgs e)
        {
            StackPanel mainPanel = new StackPanel
            {
                Uid = "clipsList",
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBox inputInitialTime = new TextBox
            {
                Uid = $"inputInitialTime_{Guid.NewGuid()}",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 150,
            };

            TextBox inputFinalTime = new TextBox
            {
                Uid = $"inputFinalTime_{Guid.NewGuid()}",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 150,
            };

            TextBox inputDescription = new TextBox
            {
                Uid = $"inputDescription_{Guid.NewGuid()}",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 400,
            };


            mainPanel.Children.Add(inputInitialTime);
            mainPanel.Children.Add(inputFinalTime);
            mainPanel.Children.Add(inputDescription);

            MainPane.Children.Add(mainPanel);

        }

        private void OpenFilePicker_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Vídeos MP4 (*.mp4)|*.mp4|Vídeos AVI (*.avi)|*.avi|Vídeos MKV (*.mkv)|*.mkv"; // Filtro de arquivos
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {

                    if (filename != null)
                    {
                        textBoxFile.Text = filename;
                    }
                    else
                    {
                        textBoxFile.Text = "Nenhum arquivo selecionado.";
                    }
                }
            }
        }

        private void ShowAlert(string title, string message)
        {
            MessageBox.Show(message, title);
        }

        public void ProcessInput_Click(object sender, RoutedEventArgs e)
        {
            var clipsList = new List<Clips>();

            try
            {
                foreach (var child in MainPane.Children)
                {
                    if (child is StackPanel stackPanel && stackPanel.Uid == "clipsList")
                    {
                        string initialTime = "";
                        string finalTime = "";
                        string description = "";

                        if (child == null)
                            return;

                        foreach (var subChild in stackPanel.Children)
                        {
                            if (subChild is TextBox textBox)
                            {
                                if (textBox.Uid.StartsWith("inputInitialTime"))
                                {
                                    initialTime = FormatToHHMMSS(textBox.Text).ToString(@"hh\:mm\:ss");
                                }
                                else if (textBox.Uid.StartsWith("inputFinalTime"))
                                {
                                    finalTime = FormatToHHMMSS(textBox.Text).ToString(@"hh\:mm\:ss");
                                }
                                else if (textBox.Uid.StartsWith("inputDescription"))
                                {
                                    description = textBox.Text;
                                }
                            }
                        }


                        if ((initialTime.Trim() == "" || finalTime.Trim() == "" || description.Trim() == ""))
                        {
                            MessageBox.Show("Alerta", "Revise os campos.");
                            return;
                        }
                        else
                        {
                            clipsList.Add(new Clips { inputInitialTime = FormatToHHMMSS(initialTime), inputFinalTime = FormatToHHMMSS(finalTime), inputDescription = description });
                        }
                    }

                }
                    if (clipsList.Count > 0)
                    {
                        try
                        {
                            ClipaVideos(clipsList);
                            ShowAlert("Alerta", "Conversão finalizada !");
                        }
                        catch { }

                    }
            }
            catch (Exception ex)
            {
                ShowAlert("Alerta", $"Revise os campos. {ex.Message}");
            }
        }


        private void ClipaVideos(List<Clips> clipsList)
        {
            try
            {
                string ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ffmpeg.exe");
                string inputVideo = textBoxFile.Text.TrimEnd('\\');
                string outputVideo = textBoxFileSave.Text.TrimEnd('\\');
               
                if (!File.Exists(ffmpegPath))
                {
                    MessageBox.Show("FFmpeg não encontrado!");
                    return;
                }

                var tasks = new List<Task>();

                var task = Task.Run(() =>
                {
                    foreach (var item in clipsList)
                    {
                        var duracaoTempo = (item.inputFinalTime - item.inputInitialTime).ToString(@"hh\:mm\:ss");
                        string comandoBat = $"-i \"{inputVideo}\" -ss {item.inputInitialTime} -t {duracaoTempo} -c copy \"{outputVideo}\\{item.inputDescription}.mp4\"";

                        //MessageBox.Show($"Comando gerado: {comandoBat}");

                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = ffmpegPath,
                                Arguments = comandoBat,
                                UseShellExecute = false,  // Não usa o shell, necessário para redirecionar a saída
                                RedirectStandardOutput = true,  // Permite capturar a saída do ffmpeg
                                RedirectStandardError = true,   // Permite capturar os erros
                                CreateNoWindow = true  // Não cria janela de console (CMD)
                            };

                            using (Process process = new Process { StartInfo = startInfo })
                            {
                                process.Start();

                                string output = process.StandardOutput.ReadToEnd();
                                string error = process.StandardError.ReadToEnd();

                                if (process.ExitCode == 0)
                                {
                                    process.Kill();
                                }
                                else
                                {
                                    MessageBox.Show($"Erro ao converter vídeo: {error}");
                                }
                            }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        static TimeSpan FormatToHHMMSS(string input)
        {
            if (TimeSpan.TryParse(input, out TimeSpan timeSpan))
            {
                return timeSpan;
            }
            else
            {
                return default(TimeSpan);
            }

        }
    }
}
