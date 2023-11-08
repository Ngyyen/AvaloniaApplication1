using System;
using SharpHook;
using Avalonia.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;
using SharpHook.Native;
using System.Net.Http;
using Newtonsoft.Json;
using LibreTranslate.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    // private static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
    TaskPoolGlobalHook hook = new TaskPoolGlobalHook();
    EventSimulator simulator = new EventSimulator();
    private const string baseurl = "http://api.mymemory.translated.net";
    private HttpClient httpClient;
    public MainWindow()
    {
        InitializeComponent();
        hook.MouseClicked += OnMouseClicked;
        // hook.MouseDragged += OnMouseDragged;
        hook.MouseReleased += OnMouseRelease;
        hook.RunAsync();
        httpClient = new HttpClient();
        // Thread t = new Thread(new ThreadStart(myFunc));
        // var task = MouseClickedDragged(CancellationTokenSource.Token);
    }
    private void Window_Closed(object? sender, System.EventArgs e)
    {
        hook.Dispose();
    }

    public string TranslateText(string input, string lang_first, string lang_second)
    {
        string url = String.Format
        ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
         lang_first, lang_second, Uri.EscapeUriString(input));
        string result = httpClient.GetStringAsync(url).Result;
        var jsonData = JsonConvert.DeserializeObject<List<dynamic>>(result);
        var translationItems = jsonData[0];
        string translation = "";
        foreach (object item in translationItems)
        {
            IEnumerable<dynamic> translationLineObject = item as IEnumerable<dynamic>;
            IEnumerator<dynamic> translationLineString = translationLineObject.GetEnumerator();
            translationLineString.MoveNext();
            translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
        }
        if (translation.Length > 1) { translation = translation.Substring(1); };
        return translation;
    }

    private async Task<string> TranslateAsync(string Text, string sourceLang, string targetLang)
    {
        string url = $"{baseurl}/get?q={Uri.EscapeDataString(Text)}&langpair={sourceLang}|{targetLang}";
        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string responseJson = await response.Content.ReadAsStringAsync();
        var translationResult = JsonConvert.DeserializeObject<TranslationResponse>(responseJson);
        
        return translationResult.TranslatedText;
    }
    public class TranslationResponse
    {
        [JsonProperty("responseStatus")]
        public int ResponseStatus { get; set; }

        [JsonProperty("responseData")]
        public TranslationData ResponseData { get; set; }

        public string TranslatedText => ResponseData?.TranslatedText;
    }

    public class TranslationData
    {
        [JsonProperty("translatedText")]
        public string TranslatedText { get; set; }
    }
    public void OnMouseClicked(object sender, MouseHookEventArgs e)
    {
        // Marshal the UI update to the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            // Update the UI elements from the UI thread
            Debug.WriteLine($"Mouse clicked at ({e.Data.X}, {e.Data.Y})");
            box.Text = $"{e.Data.X}, {e.Data.Y}";
        });
    }
    int i = 0;
    public void OnMouseRelease(object sender, MouseHookEventArgs e)
    {
        // Marshal the UI update to the UI thread
        Dispatcher.UIThread.Post(async () =>
        {
            string temp = await Clipboard.GetTextAsync();
            await Clipboard.ClearAsync();

            await Task.Delay(50);
            // Press Ctrl+C
            simulator.SimulateKeyPress(KeyCode.VcLeftControl);
            simulator.SimulateKeyPress(KeyCode.VcC);
            // Release Ctrl+C
            simulator.SimulateKeyRelease(KeyCode.VcC);
            simulator.SimulateKeyRelease(KeyCode.VcLeftControl);
            await Task.Delay(50);
            string text = await Clipboard.GetTextAsync();
            
            if (text != null && text != " ")
            {
                //box.Text = await TranslateAsync(text, "en", "vi");
                //box.Text = $"Drag detected {++i}";
                box.Text = TranslateText(text, "auto", "vi");
            }
            Clipboard.SetTextAsync(temp);
        });
    }
    
    public async void OnMouseDragged(object sender, MouseHookEventArgs e)
    {
        // Marshal the UI update to the UI thread
        Dispatcher.UIThread.Post(async () =>
        {
            string temp = await Clipboard.GetTextAsync();
            await Clipboard.ClearAsync();

            await Task.Delay(50);
            // Press Ctrl+C
            simulator.SimulateKeyPress(KeyCode.VcLeftControl);
            simulator.SimulateKeyPress(KeyCode.VcC);
            // Release Ctrl+C
            simulator.SimulateKeyRelease(KeyCode.VcC);
            simulator.SimulateKeyRelease(KeyCode.VcLeftControl);
            await Task.Delay(50);
            string text = await Clipboard.GetTextAsync();
            if (text != null)
            {
                box.Text = await TranslateAsync(text, "en", "vi");
            }
            await Clipboard.ClearAsync();
            Clipboard.SetTextAsync(temp);

            /*            if (text != null)
                        {
                            TranslationClient client = TranslationClient.CreateFromApiKey("AIzaSyDyU7lgkymST26AMXHGx3o4GEhKHUpI2yc");
                            TranslationResult result = client.TranslateText(text, LanguageCodes.Vietnamese);
                            box.Text = result.TranslatedText;
                        }*/
            // box.Text = $"Drag detected {++i}";
        });
    }  

}


