using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Data.SqlClient;
using System.Media;
using System.Net.Configuration;
using System.Configuration;

namespace BOTArbitradorBonosDolaresIOL
{
    public partial class Form1 : Form
    {
        const string sURL = "https://api.invertironline.com";
        const string SURLOper = "https://www.invertironline.com";
        string bearer;
        string refresh;
        string tokenHeader;
        string tokenForm;
        string cteCI = "0";
        string cte24 = "1";
        string strPlazo = "";

        DateTime expires;
        List<KeyValuePair<string, string>> tickers;

        SqlConnection oCnn;

		public Form1()
		{
			InitializeComponent();
			strPlazo = cteCI;

			// Asignar valores desde App.config
			txtUsuario.Text = ConfigurationManager.AppSettings["Usuario"];
			txtClave.Text = ConfigurationManager.AppSettings["Clave"];
		}

		private void AddTicker(string Codigo, string Ticker)
        {
            tickers.Add(new KeyValuePair<string, string>(Codigo, Ticker));
        }

        private void FillTickers()
        {
            tickers = new List<KeyValuePair<string, string>>();

            //Bonos
            AddTicker("99925", "AL29");
            AddTicker("99919", "AL30");
            AddTicker("99920", "AL35");
            AddTicker("99921", "AE38");
            AddTicker("99922", "AL41");
            AddTicker("99952", "GD29");
            AddTicker("99943", "GD30");
            AddTicker("99944", "GD35");
            AddTicker("99949", "GD38");
            AddTicker("99951", "GD41");
            AddTicker("99953", "GD46");

            AddTicker("99926", "AL30D");
            AddTicker("99927", "AL30C");

            AddTicker("99958", "GD30D");
            AddTicker("99963", "GD30C");

            AddTicker("100702", "AAPLD");
            AddTicker("101111", "AAPLC");

            AddTicker("100957", "AMZND");
            AddTicker("101198", "AMZNC");

            AddTicker("101123", "BBDD");
            AddTicker("101122", "BBDC");

            AddTicker("101120", "BABAD");
            AddTicker("101119", "BABAC");

            AddTicker("101144", "GOLDD");
            AddTicker("101143", "GOLDC");

            AddTicker("101158", "KOD");
            AddTicker("101157", "KOC");

            AddTicker("101162", "MELID");
            AddTicker("101161", "MELIC");

            AddTicker("101167", "MSFTD");
            AddTicker("101166", "MSFTC");

            AddTicker("101186", "TSLAD");
            AddTicker("101185", "TSLAC");
        }
        private void gbxLogin_Enter(object sender, EventArgs e)
        {

        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            Login();
        }
        private void Login()
        {
            if (expires == DateTime.MinValue)
            {
                string postData = "username=" + txtUsuario.Text + "&password=" + txtClave.Text + "&grant_type=password";
                string response;
                response = GetResponsePOST(sURL + "/token", postData);
                dynamic json = JObject.Parse(response);
                bearer = "Bearer " + json.access_token;
                expires = DateTime.Now.AddSeconds((double)json.expires_in);
                refresh = json.refresh_token;
                txtBearer.Text = json.access_token;
                //ToLog(bearer);
            }
            else
            {
                if (DateTime.Now > expires)
                {
                    string postData = "refresh_token=" + refresh + "&grant_type=refresh_token";
                    string response;
                    response = GetResponsePOST(sURL + "/token", postData);
                    dynamic json = JObject.Parse(response);
                    bearer = "Bearer " + json.access_token;
                    expires = DateTime.Now.AddSeconds((double)json.expires_in);
                    refresh = json.refresh_token;
                    txtBearer.Text = json.access_token;
                    //ToLog(bearer);
                }
            }
            tmrToken.Interval = 1000;
            tmrToken.Enabled = true;
            tmrToken.Start();
            txtStatus.Text = "Logoneado";
        }

        private string GetResponsePOST(string sUrl, string sData, string sHeaders)
        {
            WebRequest request = WebRequest.Create(sURL);
            var data = Encoding.ASCII.GetBytes(sData);
            request.Timeout = 10000;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = sData.Length;

            if (bearer != null)
            {
                request.Headers.Add("Authorization", bearer);
            }
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        private string GetResponsePOST(string sURL, string sData)
        {
            WebRequest request = WebRequest.Create(sURL);
            var data = Encoding.ASCII.GetBytes(sData);
            request.Timeout = 10000;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = sData.Length;

            if (bearer != null)
            {
                request.Headers.Add("Authorization", bearer);
            }
            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                WebResponse response = request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InicializarDatos();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = (SettingsSection)config.GetSection("system.net/settings");
            settings.HttpWebRequest.UseUnsafeHeaderParsing = true;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("system.net/settings");
        }

        private void InicializarDatos()
        {
            txtPresupuesto.Text = "1000";
            txtComision.Text = "0";
            tbComision.Value = 0;
            FillCombo(ref cboBonoIda);
            cboBonoIda.Text = "GD30";
            FillCombo(ref cboBonoVuelta);
            cboBonoVuelta.Text = "AL30";
            cboRefresco.Items.Clear();
            for (int i = 1; i < 21; i++)
            {
                cboRefresco.Items.Add(i);
            }
            cboRefresco.Text = "5";

            cboUmbral.Items.Clear();
            for (double j=0;j<250;j++)
            {
                cboUmbral.Items.Add(j.ToString());
            }
            cboUmbral.Text = "10";

            txtStatus.Text = "Desocupado";
            FillTickers();
            cboPlazo.Items.Clear();
            cboPlazo.Items.Add("0");
            cboPlazo.Items.Add("1");
            cboPlazo.Text = "0";
		}

        private void FillCombo(ref ComboBox cbo)
        {
            cbo.Items.Clear();
            cbo.Items.AddRange(new string[] { "SY5","SJ5","SO5", "AL29", "GD29", "AL30", "GD30", "AL35", "GD35", "AE38", "GD38", "AL41", "GD41", "GD46","AAPL","AMZN", "BABA", "BBD", "GOLD","KO","MELI","MSFT","TSLA" });
        }

        private void tmrToken_Tick_1(object sender, EventArgs e)
        {
            txtVencimiento.Text = Math.Round((expires - DateTime.Now).TotalSeconds).ToString();
        }

        private string GetResponseGET(string sURL, string sHeader)
        {
            WebRequest request = WebRequest.Create(sURL);
            request.Timeout = 10000;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", sHeader);
            try
            {
                WebResponse response = request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        private void btnAPI_Click(object sender, EventArgs e)
        {
            tmrRefresh.Interval = int.Parse(cboRefresco.Text) * 1000;
            tmrRefresh.Enabled = true;
            tmrRefresh.Start();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {

            string response;
            dynamic cotizaciones;
            dynamic cotizacion;
            dynamic punta;
            Boolean bPunta1 = false;
            Boolean bPunta2 = false;
            Boolean bPunta3 = false;
            Boolean bPunta4 = false;

            string BonoIda;
            string BonoVuelta;
            int QDVentaD = 0;
            double PVentaD = 0;
            int QDCompraC = 0;
            double PCompraC = 0;
            int QDVentaC = 0;
            double PVentaC = 0;
            int QDCompraD = 0;
            double PCompraD = 0;

            Login();

            if (chkDB.Checked)
            {
                oCnn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Trading;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                oCnn.Open();
            }

            BonoIda = cboBonoIda.Text;

            txtStatus.Text = "Obteniendo " + BonoIda;
            Application.DoEvents();
            response = GetResponseGET(sURL + "/api/v2/{Mercado}/Titulos/{Simbolo}/Cotizacion?mercado=bcba&simbolo=" + BonoIda + "D&model.simbolo=" + BonoIda + "D&model.mercado=bCBA&model.plazo=t" + strPlazo, bearer);
            if (response.Contains("Error") || response.Contains("tiempo de espera"))
            { Application.DoEvents(); }
            else
            {
                cotizaciones = JArray.Parse("[" + response + "]");
                cotizacion = cotizaciones[0];
                if (cotizacion.puntas.Count > 0)
                {
                    punta = cotizacion.puntas[0];
                    QDVentaD = (int)punta.cantidadVenta;
                    PVentaD = (double)punta.precioVenta;
                    if (QDVentaD > 0 && PVentaD > 0)
                    { bPunta1 = true; }
                }
            }

            response = GetResponseGET(sURL + "/api/v2/{Mercado}/Titulos/{Simbolo}/Cotizacion?mercado=bcba&simbolo=" + BonoIda + "C&model.simbolo=" + BonoIda + "C&model.mercado=bCBA&model.plazo=t" + strPlazo, bearer);
            if (response.Contains("Error") || response.Contains("tiempo de espera"))
            { Application.DoEvents(); }
            else
            {
                cotizaciones = JArray.Parse("[" + response + "]");
                cotizacion = cotizaciones[0];
                if (cotizacion.puntas.Count > 0)
                {
                    punta = cotizacion.puntas[0];
                    QDCompraC = (int)punta.cantidadCompra;
                    PCompraC = (double)punta.precioCompra;
                    if (QDCompraC > 0 && PCompraC > 0)
                    { bPunta2 = true; }
                }
            }

            BonoVuelta= cboBonoVuelta.Text;

            txtStatus.Text = "Obteniendo " + BonoVuelta;
            Application.DoEvents();

            response = GetResponseGET(sURL + "/api/v2/{Mercado}/Titulos/{Simbolo}/Cotizacion?mercado=bcba&simbolo=" + BonoVuelta + "C&model.simbolo=" + BonoVuelta + "C&model.mercado=bCBA&model.plazo=t" + strPlazo, bearer);
            if (response.Contains("Error") || response.Contains("tiempo de espera"))
            { Application.DoEvents(); }
            else
            {
                cotizaciones = JArray.Parse("[" + response + "]");
                cotizacion = cotizaciones[0];
                if (cotizacion.puntas.Count > 0)
                {
                    punta = cotizacion.puntas[0];
                    QDVentaC = (int)punta.cantidadVenta;
                    PVentaC = (double)punta.precioVenta;
                    if (QDVentaC > 0 && PVentaC > 0)
                    { bPunta3 = true; }
                }
            }

            response = GetResponseGET(sURL + "/api/v2/{Mercado}/Titulos/{Simbolo}/Cotizacion?mercado=bcba&simbolo=" + BonoVuelta + "D&model.simbolo=" + BonoVuelta + "D&model.mercado=bCBA&model.plazo=t" + strPlazo, bearer);
            if (response.Contains("Error") || response.Contains("tiempo de espera"))
            { Application.DoEvents(); }
            else
            {
                cotizaciones = JArray.Parse("[" + response + "]");
                cotizacion = cotizaciones[0];
                if (cotizacion.puntas.Count > 0)
                {
                    punta = cotizacion.puntas[0];
                    QDCompraD = (int)punta.cantidadCompra;
                    PCompraD = (double)punta.precioCompra;
                    if (QDCompraD > 0 && PCompraD > 0)
                    { bPunta4 = true; }
                }
            }

            if (bPunta1 && bPunta2 && bPunta3 && bPunta4)
            {
                txtQDVentaD.Text = QDVentaD.ToString();
                txtPVentaD.Text = PVentaD.ToString();
                txtQDCompraC.Text = QDCompraC.ToString();
                txtPCompraC.Text = PCompraC.ToString();
                txtRatioIda.Text = string.Empty;
                txtQDVentaC.Text = QDVentaC.ToString();
                txtPVentaC.Text = PVentaC.ToString();
                txtQDCompraD.Text = QDCompraD.ToString();
                txtPCompraD.Text = PCompraD.ToString();
                txtRatioVuelta.Text = string.Empty;

                double ratioIda = Math.Round(100-((PVentaD / PCompraC)*100),2);
                double ratioVuelta = Math.Round(100-((PVentaC / PCompraD)*100), 2);
                double diferencia =  Math.Round((((100+ratioIda)/100)*((100+ratioVuelta)/100)-1)*100,2);

                double umbral =(double.Parse(cboUmbral.Text))/100;
                if (diferencia>umbral && chkBeep.Checked)
                {
                    SystemSounds.Beep.Play();
                    //Arbitrar();
                }

                if (chkBeep.Checked && ratioIda>-2)
                {
                    //SystemSounds.Beep.Play();
                }

                if (chkBeep.Checked && ratioVuelta > 3)
                {
                    //SystemSounds.Beep.Play();
                }

                txtRatioIda.Text = ratioIda.ToString();
                txtRatioVuelta.Text = ratioVuelta.ToString();
                try
                {
                    if (chkDB.Checked)
                    { 
                        string sComando = "Insert into ArbitrajeBonosEspecies([BonoIda], [QVentaBonoIda], [PVentaBonoIda], [QCompraBonoIda], [PCompraBonoIda]," +
                                    "[BonoVuelta], [QVentaBonoVuelta], [PVentaBonoVuelta], [QCompraBonoVuelta], [PCompraBonoVuelta]) " +
                                    "values ('" + BonoIda + "'," + QDVentaD.ToString() + "," + PVentaD.ToString().Replace(",", ".") + "," + QDCompraC.ToString() + "," + PCompraC.ToString().Replace(",", ".")
                                    + ",'" + BonoVuelta + "'," + QDVentaC.ToString() + "," + PVentaC.ToString().Replace(",", ".") + "," + QDCompraD.ToString() + "," + PCompraD.ToString().Replace(",", ".") + ")";
                        SqlCommand oCmd = new SqlCommand(sComando, oCnn);
                        oCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                }
                string ahora = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
                crtRatios.Series["Ida"].Points.AddXY(ahora, ratioIda*(-1));
                crtRatios.Series["Vuelta"].Points.AddXY(ahora, ratioVuelta);
                crtRatios.Series["Diferencia"].Points.AddXY(ahora, diferencia);

                if (crtRatios.Series["Ida"].Points.Count > 20)
                { 
                    crtRatios.Series["Ida"].Points.RemoveAt(0);
                    crtRatios.Series["Vuelta"].Points.RemoveAt(0); 
                    crtRatios.Series["Diferencia"].Points.RemoveAt(0); 
                }

            }
            else
            {
                txtQDVentaD.Text = "";
                txtPVentaD.Text = "";
                txtQDCompraC.Text = "";
                txtPCompraC.Text = "";
                txtQDVentaC.Text = "";
                txtPVentaC.Text = "";
                txtQDCompraD.Text = "";
                txtPCompraD.Text = "";
            }
            txtStatus.Text = "Desocupado";
            Application.DoEvents();
        }

        private void crtRatios_Click(object sender, EventArgs e)
        {

        }

        private void Arbitrar(bool ida, bool vuelta)
        {
            string sBonoIda = cboBonoIda.Text;
            string sBonoVuelta = cboBonoVuelta.Text;

            string url_prelogin = "https://micuenta.invertironline.com/ingresar?url=https://iol.invertironline.com/&intencion=0";
            string sCookies="";
            
            txtStatus.Text = "Iniciando arbitraje";
            Application.DoEvents();

            HttpWebRequest request = WebRequest.CreateHttp(url_prelogin);
            WebResponse response;
            request.Timeout = 15000;
            request.Method = "GET";
            request.Host = "micuenta.invertironline.com";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0";
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "deflate");
            request.Headers.Add("DNT", "1");
            request.Headers.Add("Cookie", "anonymous=true; intencionApertura=0; i18n.langtag=es-AR; isMobile=0");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Headers.Add("Cache-Control", "max-age=0");

            try
            {
                response = request.GetResponse();
                string TokenHeader = StringBetween(response.Headers.Get("Set-Cookie"), "__RequestVerificationToken=", ";");
                string TokenForm = StringBetween(new StreamReader(response.GetResponseStream()).ReadToEnd(), @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""", @""" />");


                string url_postlogin = "https://micuenta.invertironline.com/Ingresar";
                string body = "__RequestVerificationToken=" + TokenForm;
                body = body + "&UrlRedireccion=&Usuario=" + txtUsuario.Text;
                body = body + "&Password=" + txtClave.Text;
                request = WebRequest.CreateHttp(url_postlogin);
                request.Timeout = 10000;
                request.Method = "POST";
                request.Host = "micuenta.invertironline.com";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Referer = "https://micuenta.invertironline.com/Ingresar";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "deflate");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Cookie", "anonymous=true; intencionApertura=0;__RequestVerificationToken=" + TokenHeader);
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Cache-Control", "no-cache");
                request.AllowAutoRedirect = false;

                request.ContentLength = body.Length;
                byte[] data = Encoding.ASCII.GetBytes(body);
                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);

                response = request.GetResponse();
                string dafcms = StringBetween(response.Headers.Get("Set-Cookie"), "5dafCMS575d0c=", ";");
                string sidglobal = StringBetween(response.Headers.Get("Set-Cookie"), "__sidglobal=", ";");

                sCookies = response.Headers.Get("set-Cookie");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (sCookies==""){ return; }

            string bono;
            double precio;
            int cantidad;
            bool resultado = false;

            //Paso 1
            if (ida)
            {
                bono = cboBonoIda.Text + "D";
                precio = double.Parse(txtPVentaD.Text);
                cantidad = int.Parse(txtQVentaD.Text);
                resultado = Comprar(bono, precio, cantidad, sCookies);

                //Paso2
                if (resultado)
                {
                    bono = cboBonoIda.Text + "C";
                    precio = double.Parse(txtPCompraC.Text);
                    cantidad = int.Parse(txtQCompraC.Text);
                    resultado = Vender(bono, precio, cantidad, sCookies);
                    if (resultado==false)
                    {
                        MessageBox.Show("Falló en Paso 2: venta del " + bono, "Fallo");
                    }
                }
                else
                {
                    MessageBox.Show("Falló en Paso 1: compra del " + bono, "Fallo");
                }
            }


            //Paso3
            if ((ida==false && vuelta==true) || (ida==true && vuelta==true && resultado == true))
            {
                bono = cboBonoVuelta.Text + "C";
                precio = double.Parse(txtPVentaC.Text);
                cantidad = int.Parse(txtQVentaC.Text);
                resultado = Comprar(bono, precio, cantidad, sCookies);

                //Paso4
                if (resultado)
                {
                    bono = cboBonoVuelta.Text + "D";
                    precio = double.Parse(txtPCompraD.Text);
                    cantidad = int.Parse(txtQCompraD.Text);
                    resultado = Vender(bono, precio, cantidad, sCookies);
                    if (resultado == false)
                    {
                        MessageBox.Show("Falló en Paso 4: venta del " + bono, "Fallo");
                    }
                }
                else
                {
                    MessageBox.Show("Falló en Paso 3: compra del " + bono, "Fallo");
                }

            }

        }

        private Boolean Vender(string titulo, double precio, int cantidad, string cookies)
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string url;
            HttpWebRequest request;
            WebResponse response;
            string sCook;
            string validez;
            string body;
            byte[] data;
            string s;
            Stream stream;

            try
            {
                //Paso 1
                txtStatus.Text = "Iniciando venta";
                Application.DoEvents();
                url = "https://iol.invertironline.com/Operar/Vender";
                request = WebRequest.CreateHttp(url);
                request.Timeout = 10000;
                request.Method = "POST";
                request.AllowAutoRedirect = true;
                request.Host = "iol.invertironline.com";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                //request.Referer = "https://micuenta.invertironline.com/Ingresar";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "deflate");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Cookie", cookies);
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Cache-Control", "max-age=0");

                validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";

                body = "IdPlazo=3&IdSimbolo=" + GetCodigoByTicker(titulo);
                body = body + "&Cantida=" + cantidad.ToString();
                body = body + "&Modalidad=PrecioLimite";
                body = body + "&PrecioLimite=" + precio.ToString();
                body = body + "&Validez=" + validez;
                data = Encoding.ASCII.GetBytes(body);
                request.ContentLength = body.Length;
                stream = request.GetRequestStream();
                { stream.Write(data, 0, data.Length); }
                response = request.GetResponse();
                sCook = response.Headers.Get("Set-Cookie");
                s = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response = request.GetResponse();
                sCook = response.Headers.Get("Set-Cookie");

                //Paso 3
                url = "https://iol.invertironline.com/Operar/ConfirmarVenta";
                request = WebRequest.CreateHttp(url);
                request.Timeout = 10000;
                request.Method = "POST";
                request.Host = "iol.invertironline.com";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Referer = "https://iol.invertironline.com/Operar/Vender";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "deflate");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Cookie", cookies + ";" + sCook);
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Cache-Control", "max-age=0");
                validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                body = "DeclaracionJuradoEstaAceptada=true&DeclaracionJuradoEstaAceptada=false";
                body = body + "&Password=" + txtClave.Text;
                body = body + "&IdSimbolo=" + GetCodigoByTicker(titulo);
                body = body + "&Modalidad=PrecioLimite";
                body = body + "&IdPlazo=3";
                body = body + "&PrecioLimite=" + precio.ToString();
                body = body + "&Cantidad=" + cantidad.ToString();
                body = body + "&Validez=" + validez;
                body = body + "&Volver=true";

                data = Encoding.ASCII.GetBytes(body);
                request.ContentLength = body.Length;

                stream = request.GetRequestStream();
                { stream.Write(data, 0, data.Length); }
                response = request.GetResponse();
                s = new StreamReader(response.GetResponseStream()).ReadToEnd();
                txtStatus.Text = "Fin venta";
                Application.DoEvents();

                return true;
            }

            catch (Exception e)
            {
                return false;
            }

        }

        private Boolean Comprar(string titulo, double precio, int cantidad, string cookies)
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            string url;
            HttpWebRequest request;
            WebResponse response;
            string sCook;
            string validez;
            string body;
            byte[] data;
            string s;
            Stream stream;

            try
            {
                //Paso 1
                txtStatus.Text = "Iniciando compra";
                Application.DoEvents();
                url = "https://iol.invertironline.com/Operar/Comprar";
                request = WebRequest.CreateHttp(url);
                request.Timeout = 10000;
                request.Method = "GET";
                request.AllowAutoRedirect = true;
                request.Host = "iol.invertironline.com";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                //request.Referer = "https://micuenta.invertironline.com/Ingresar";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "deflate");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Cookie", cookies);
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Cache-Control", "max-age=0");
                response = request.GetResponse();
                sCook = response.Headers.Get("Set-Cookie");

                //Paso 2
                txtStatus.Text = "Login ok";
                Application.DoEvents();
                request = WebRequest.CreateHttp(url);
                request.Timeout = 15000;
                request.Method = "POST";
                request.Host = "iol.invertironline.com";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "deflate");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Cookie", cookies + ";" + sCook);
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Cache-Control", "max-age=0");
                validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                body = "pais=54&PlazoOperacion=3&IdSimbolo=" + GetCodigoByTicker(titulo);
                body = body + "&Simbolo=" + titulo;
                body = body + "&CantidadMonto=2&ValorCantidad=" + cantidad.ToString();
                body = body + "&Modalidad=1&ValorPrecioLimite=" + precio.ToString();
                body = body + "&Validez=" + validez;

                data = Encoding.ASCII.GetBytes(body);
                request.ContentLength = body.Length;

                stream = request.GetRequestStream();
                { stream.Write(data, 0, data.Length); }
                response = request.GetResponse();
                sCook = response.Headers.Get("Set-Cookie");
                s = new StreamReader(response.GetResponseStream()).ReadToEnd();

                //Paso 3
                txtStatus.Text = "Paso final de compra";
                Application.DoEvents();
                url = "https://iol.invertironline.com/Operar/ConfirmarCompra";
                request = WebRequest.CreateHttp(url);
                request.Timeout = 10000;
                request.Method = "POST";
                request.Host = "iol.invertironline.com";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Referer = "https://iol.invertironline.com/Operar/Comprar";
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "deflate");
                request.Headers.Add("DNT", "1");
                request.Headers.Add("Cookie", cookies + ";" + sCook);
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Headers.Add("Cache-Control", "max-age=0");
                validez = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "T17:59:59.000Z";
                body = "alarma=false&StopLoss=false&SubeBaja=2&AlarmValor=&AlarmCantidad=1";
                body = body + "&AlarmPrecioLimite=&IdModalidad=1&volver=true&Pais=54&TipoOperacion=Individual";
                body = body + "&dummy=" + txtUsuario.Text + "&Password=" + txtClave.Text;
                body = body + "&PlazoOperacion=3&IdSimbolo=" + GetCodigoByTicker(titulo);
                body = body + "&ValorCantidad=" + cantidad.ToString();
                body = body + "&ValorMonto=&Modalidad=1&ValorPrecioLimite=" + precio.ToString();
                body = body + "&Validez=" + validez;

                data = Encoding.ASCII.GetBytes(body);
                request.ContentLength = body.Length;

                stream = request.GetRequestStream();
                { stream.Write(data, 0, data.Length); }
                response = request.GetResponse();
                s = new StreamReader(response.GetResponseStream()).ReadToEnd();

                txtStatus.Text = "Fin compra";
                Application.DoEvents();

                return true;
            }

            catch (Exception e)
            {
                return false;
            }
        }


        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {return true;}

        private string GetTickerByCodigo(object Codigo)
        {
            KeyValuePair<string, string> ticker = tickers.Where(t => t.Key == Codigo.ToString()).FirstOrDefault();
            return ticker.Value;
        }

        private string GetCodigoByTicker(string Ticker)
        {
            KeyValuePair<string, string> ticker = tickers.Where(t => t.Value == Ticker).FirstOrDefault();
            return ticker.Key;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            Arbitrar(true,true);
        }
        public string StringBetween(string Qry, string FTag, string STag)
        {
            string StringBetweenRet = default;
            int L;
            L = Qry.IndexOf(FTag) + FTag.Length;
            StringBetweenRet = Qry.Substring(L, Qry.IndexOf(STag, L) - L);
            return StringBetweenRet;
            //El crédito por esta función es de @MarceloColom.
        }

        private void tbComision_Scroll(object sender, EventArgs e)
        {
            txtComision.Text = tbComision.Value.ToString();
        }

        private void txtComision_TextChanged(object sender, EventArgs e)
        {
            int valor;
            int.TryParse(txtComision.Text,out valor);
            tbComision.Value = valor;
        }

        private void txtArbitrarIda_Click(object sender, EventArgs e)
        {
            Arbitrar(true, false);
        }

        private void txtArbitrarVuelta_Click(object sender, EventArgs e)
        {
            Arbitrar(false, true);
        }

        private void btnWebsockets_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Funcionalidad pendiente...", "Aviso");
        }

        private void cboRefresco_SelectedIndexChanged(object sender, EventArgs e)
        {
            tmrRefresh.Interval = int.Parse(cboRefresco.Text) * 1000;
        }

		private void cboPlazo_SelectedIndexChanged(object sender, EventArgs e)
		{
			strPlazo = cboPlazo.Text;
		}
	}
}
