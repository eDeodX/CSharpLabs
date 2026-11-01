using WeatherCore;
namespace Lab09Weather
{
    public class WeatherForm : Form
    {
        private ListBox cities = new ListBox();
        private Button load = new Button();
        private TextBox key = new TextBox();
        private Label result = new Label();
        private CheckBox demo = new CheckBox();
        public WeatherForm()
        {
            Text = "ЛР9 — Погода";
            ClientSize = new Size(620, 440);
            cities.SetBounds(15, 15, 280, 405);
            key.SetBounds(310, 45, 290, 25);
            key.UseSystemPasswordChar = true;
            key.Text = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY") ?? "";
            Label caption = new Label();
            caption.Text = "Ключ OpenWeather:";
            caption.SetBounds(310, 20, 280, 20);
            demo.Text = "Учебный ответ JSON";
            demo.SetBounds(310, 85, 290, 25);
            load.Text = "Загрузить погоду";
            load.SetBounds(310, 130, 290, 40);
            result.SetBounds(310, 190, 290, 210);
            Controls.AddRange(new Control[] { cities, key, caption, demo, load, result });
            load.Click += LoadWeather;
            try
            {
                cities.DataSource = City.Load(Path.Combine(AppContext.BaseDirectory, "Data", "city.txt"));
            }
            catch (Exception e)
            {
                result.Text = "Ошибка списка городов: " + e.Message;
                load.Enabled = false;
            }
        }
        private async void LoadWeather(object sender, EventArgs e)
        {
            City city = cities.SelectedItem as City;
            if (city == null) return;
            string apiKey = key.Text.Trim();
            if (!demo.Checked && apiKey == "")
            {
                result.Text = "Введи ключ OpenWeather.";
                return;
            }
            bool useDemo = demo.Checked;
            load.Enabled = false;
            result.Text = "Загрузка: " + city.Name;
            try
            {
                string weather;
                if (useDemo)
                {
                    string path = Path.Combine(AppContext.BaseDirectory, "Data", "weather_demo.json");
                    weather = WeatherClient.Parse(await File.ReadAllTextAsync(path));
                }
                else
                {
                    using (WeatherClient client = new WeatherClient())
                        weather = await client.LoadAsync(city, apiKey);
                }
                result.Text = city.Name + Environment.NewLine + weather;
                if (useDemo) result.Text += Environment.NewLine + "Учебные данные.";
            }
            catch (Exception error) { result.Text = "Ошибка: " + error.Message; }
            finally { load.Enabled = true; }
        }
    }
}
