using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace WpfApp10;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    FileDialog? dialog = null;
    CancellationTokenSource cancellation = new CancellationTokenSource();
    public List<string> FileText { get; set; } = new List<string>();

    public MainWindow()
    {
        InitializeComponent();
        progressBarStream.Minimum = 0;
        progressBarStream.Maximum = 100;
    }

    private void btnFile_Click(object sender, RoutedEventArgs e)
    {
        dialog = new OpenFileDialog();
        dialog.Filter = "Text Files |*.txt";

        var result = dialog.ShowDialog();

        if (result == true)
            txtBlockFileName.Text = dialog.FileName;
    }


    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtBlockFileName.Text))
        {
            MessageBox.Show("Select File","",MessageBoxButton.OK , MessageBoxImage.Error);
            return;
        }

        if (radioBtnEncrypt.IsChecked != true && radioBtnDecrypt.IsChecked != true)
        {
            MessageBox.Show("Select Mode", "", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (string.IsNullOrEmpty(txtBoxPassword.Text))
        {
            MessageBox.Show("Enter Password", "", MessageBoxButton.OK, MessageBoxImage.Error);
            return;

        }

        cancellation = new();
        if (radioBtnEncrypt.IsChecked is true)
        {

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Dispatcher.Invoke(() => EncryptFile());
                }
                catch (Exception)
                {
                    MessageBox.Show("Encrypt Faild");
                }
            });
        }
        else
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Dispatcher.Invoke(() => DencryptFile());
                }
                catch (Exception)
                {
                    MessageBox.Show("Decrypt Faild");
                }
            });
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        cancellation.Cancel();
    }



    public string  XORCipher(string data, string key)
    {
        int dataLen = data.Length;
        int keyLen = key.Length;
        char[] result = new char[dataLen];

        for (int i = 0; i < dataLen; ++i)
            result[i] = (char)(data[i] ^ key[i % keyLen]);

        return new string(result);
    }

    private void DencryptFile()
    {
        using (StreamReader sr = new StreamReader(txtBlockFileName.Text))
        {
            string line;
            FileText.Clear();

            while ((line = sr.ReadLine()) != null)
                FileText.Add(line);
        }

        File.WriteAllText(txtBlockFileName.Text, String.Empty);

        using (StreamWriter sw = new StreamWriter(txtBlockFileName.Text))
        {
            foreach (var item in FileText)
            {
                if (cancellation.IsCancellationRequested)
                    break;

                Thread.Sleep(500);
                Dispatcher.Invoke(new Action(() => progressBarStream.Value += 100 / FileText.Count));

                var encrypt = XORCipher(item, txtBoxPassword.Text);
                sw.WriteLine(encrypt);
            }
        }
        if (!cancellation.IsCancellationRequested)
        {
            Dispatcher.Invoke(new Action(() => progressBarStream.Value = 100));

            MessageBox.Show("Decrypt  is successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            progressBarStream.Value = 0;
        }
    }

    private  void  EncryptFile()
    {
        using (StreamReader sr = new StreamReader(txtBlockFileName.Text))
        {
            string line;

            FileText.Clear();

            while ((line = sr.ReadLine()) != null)
                FileText.Add(line);
        }

        File.WriteAllText(txtBlockFileName.Text, String.Empty);

         using (StreamWriter sw = new StreamWriter(txtBlockFileName.Text))
        {
            foreach (var item in FileText)
            {
                if (cancellation.IsCancellationRequested)
                    break;

                Dispatcher.Invoke(new Action(() => progressBarStream.Value += 100 / FileText.Count));

                var encrypt = XORCipher(item, txtBoxPassword.Text);
                sw.WriteLine(encrypt);
                Thread.Sleep(500);
            }
        }

        if (!cancellation.IsCancellationRequested)
        {
            Dispatcher.Invoke(new Action(() => progressBarStream.Value = 100));

            MessageBox.Show("Encrypt  is successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            progressBarStream.Value = 0;
        }
    }
}
