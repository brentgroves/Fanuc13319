using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

/*
 * 
 I could not find a good MQTT console app example the only one that work seemed to be one from a Windows form example combined with a managed client console application
that did not work.
https://dzone.com/articles/mqtt-publishing-and-subscribing-messages-to-mqtt-b
https://github.com/chkr1011/MQTTnet/wiki
* 
 * 
 */

namespace Fanuc13319
{
    class Program
    {
        // short _ret = 0;  // Stores our return value
        private IManagedMqttClient managedMqttClientPublisher;
        string fanucIP = "10.1.90.4";
        string mqttServer = "10.1.1.83";
        ushort fanucHandle = 0;
        int partCounter = 34;

        MqttFactory mqttFactory;
        /// <summary>
        /// The managed publisher client.
        /// </summary>
        //  static IManagedMqttClient managedMqttClientPublisher;


        static void Main(string[] args)
        {
            // ushort handle = 0;
            Program myObj = new Program();

            // If we specified an ip address, get it from the args
            if (args.Length > 0)
                myObj.fanucIP = args[0];
            else
                return;

            // myObj.Test();
            // return;
            myObj.MQTTConnect();


            var timer = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = 10000
            };

            timer.Elapsed += myObj.TimerElapsed;


            // myObj.GetCounter(526);
            // System.Threading.Thread.Sleep( Timeout.InfiniteTimeSpan);
            // System.Threading.Thread.Sleep(35000);
            // Put in OnExit() cleanup handler
            Console.ReadLine();
            myObj.MQTTStop();
            myObj.FanucCleanup();
            // Focas1.cnc_freelibhndl(myObj.fanucHandle);  // Not sure if I should free this handle indescriminately



        }

        private void Test()
        {
            DateTime localDate = DateTime.Now;
            Console.WriteLine(localDate.ToString("yyyy-MM-dd HH:mm:ss"));

//            string mariaDb = DateTime.Now.ToString("yyyy-MM-dd"); // or "dd" for day
            string mariaDb = new DateTime(2012, 12, 5, 1, 1, 1).ToString("yyyy-MM-dd HH:mm:ss"); // or "dd" for day
            Console.WriteLine(mariaDb.ToString());



            // 2015 is year, 12 is month, 25 is day  
            DateTime date1 = new DateTime(2015, 12, 25);
            Console.WriteLine(date1.ToString()); // 12/25/2015 12:00:00 AM    

            // 2015 - year, 12 - month, 25 – day, 10 – hour, 30 – minute, 50 - second  
            DateTime date2 = new DateTime(2012, 12, 25, 10, 30, 50);
            Console.WriteLine(date1.ToString());// 12/25/2015 10:30:00 AM }  
        }
        private void FanucCleanup()
        {
            // Focas1.cnc_freelibhndl(myObj.fanucHandle);  // Not sure if I should free this handle indescriminately
            Console.WriteLine("All Fanuc Handles have been closed.");

        }
        private async void MQTTStop()
        {
            await managedMqttClientPublisher.StopAsync();
            managedMqttClientPublisher = null;
            Console.WriteLine("MQTT Connection has been stopped");

        }

        private async void MQTTConnect()
        {
            mqttFactory = new MqttFactory();
            var tlsOptions = new MqttClientTlsOptions
            {
                UseTls = false,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true
            };
            var options = new MqttClientOptions
            {
                ClientId = "ClientPublisher",
                ProtocolVersion = MqttProtocolVersion.V311,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = "10.1.1.83",
                    Port = 1882,
                    TlsOptions = tlsOptions
                }
            };
            if (options.ChannelOptions == null)
            {
                throw new InvalidOperationException();
            }
            options.Credentials = new MqttClientCredentials
            {
                Username = "username",
                Password = Encoding.UTF8.GetBytes("password")
            };
            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(5);
            managedMqttClientPublisher = mqttFactory.CreateManagedMqttClient();
            managedMqttClientPublisher.UseApplicationMessageReceivedHandler(HandleReceivedApplicationMessage);
            managedMqttClientPublisher.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnPublisherConnected);
            managedMqttClientPublisher.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnPublisherDisconnected);
            await managedMqttClientPublisher.StartAsync(
                new ManagedMqttClientOptions
                {
                    ClientOptions = options
                });


        }

        /// <summary>
        /// The method that handles the timer events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            GetCounter(526);
            // test();
            MQTTPublish();

        }

        async void MQTTPublish()
        {
            try
            {
                string json = @"{
                  'Name': 'Bad Boys',
                  'ReleaseDate': '1995-4-7T00:00:00',
                  'Genres': [
                    'Action',
                    'Comedy'
                  ]
                }";

                string test = @"{
                  'updateId': 5,
                  'nodeId': 'ns=2;s=cnc362.cnc362.Cycle_Counter_Shift_SL',
                  'name':'Cycle_Counter_Shift_SL',
                  'plexus_Customer_No':'310507',
                  'pcn': 'Avilla',
                  'workcenter_Key': '61314',
                  'workcenter_Code': 'Honda Civic cnc 359 362',  
                  'cnc': '362',
                  'value': 0,
                  'transDate': '2020-06-29 00:00:00'
                }";

                Node node = new Node();
                Console.WriteLine("node.updateId {0}", node.updateId);
                Console.WriteLine("node.nodeId {0}", node.nodeId);
                Console.WriteLine("node.name {0}", node.name);
                // https://stackoverflow.com/questions/7574606/left-function-in-c-sharp/7574645

                node.value = partCounter;
                DateTime localDate = DateTime.Now;
                string transDate = localDate.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine(transDate);
                node.transDate = transDate;


                // const transDate = moment(new Date()).format("YYYY-MM-DDTHH:mm:ss");
                // yyyy-MM
                string json2 = JsonConvert.SerializeObject(node);
                Console.WriteLine("json2=> {0}", json2);


                var payload = Encoding.UTF8.GetBytes(json2);

                //                var payload = Encoding.UTF8.GetBytes(partCounter);
                var message = new MqttApplicationMessageBuilder().WithTopic("Kep13319").WithPayload(payload).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce).WithRetainFlag().Build();

                if (managedMqttClientPublisher != null)
                {
                    await managedMqttClientPublisher.PublishAsync(message);
                    Console.WriteLine("Published Message=>{0}", message);
                }
                else
                {
                    throw new Exception("MQTTPublish => Not connected to MQTT server");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }




        /* number is variable number to be read. 
         * GetCounter(526);//cnc362 10.1.90.4
           GetCounter(511);//cnc422 10.1.90.5
           GetCounter(501);//cnc422 10.1.90.2
         * 
         */
        short GetCounter(short number)
        {
            short ret = -1;
            try
            {
                ret = Focas1.cnc_allclibhndl3(fanucIP, 8193, 6, out fanucHandle);
                if (ret == Focas1.EW_OK)
                {
                    Console.WriteLine("We are connected!");
                }
                else
                {
                    // Console.WriteLine("There was an error connecting. Return value: " + _ret);
                    throw new Exception("There was an error connecting. Return value: " + ret);
                }

                Focas1.ODBM macro = new Focas1.ODBM();
                string strVal;
                ret = Focas1.cnc_rdmacro(fanucHandle, number, 10, macro);
                if (ret == Focas1.EW_OK)
                {
                    // mcr_val = 406000000
                    // dec_val = 6
                    // value = 406.000000
                    strVal = string.Format("{0:d9}", Math.Abs(macro.mcr_val));
                    if (0 < macro.dec_val) strVal = strVal.Insert(9 - macro.dec_val, ".");
                    if (macro.mcr_val < 0) strVal = "-" + strVal;
                    Console.WriteLine("partCounter={0}", strVal);
                    decimal decimalVal;
                    decimalVal = Convert.ToDecimal(strVal);
                    Console.WriteLine("String converted to decimal = {0} ", decimalVal);

                    partCounter = Convert.ToInt32(decimalVal);
                    //                    partCounter2 = macro.mcr_val;
                    Console.WriteLine("partCounter.int= {0}", partCounter);
                }
                else
                {
                    throw new Exception("There was an error reading the macro variable. Return value: " + ret);
                    // Console.WriteLine("**********");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception => {0}", e);
            }
            finally
            {
                // Free the Focas handle
                Focas1.cnc_freelibhndl(fanucHandle);
                Console.WriteLine("Handle has been freed");
            }
            return ret;

        }

        /// <summary>
        /// Handles the publisher connected event.
        /// </summary>
        /// <param name="x">The MQTT client connected event args.</param>
        private static void OnPublisherConnected(MqttClientConnectedEventArgs x)
        {
            Console.WriteLine("Publisher Connected");
        }

        /// <summary>
        /// Handles the publisher disconnected event.
        /// </summary>
        /// <param name="x">The MQTT client disconnected event args.</param>
        private static void OnPublisherDisconnected(MqttClientDisconnectedEventArgs x)
        {
            Console.WriteLine("Publisher Disconnected");
        }

        /// <summary>
        /// Handles the received application message event.
        /// </summary>
        /// <param name="x">The MQTT application message received event args.</param>
        private void HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs x)
        {
            var item = $"Timestamp: {DateTime.Now:O} | Topic: {x.ApplicationMessage.Topic} | Payload: {x.ApplicationMessage.ConvertPayloadToString()} | QoS: {x.ApplicationMessage.QualityOfServiceLevel}";
            // this.BeginInvoke((MethodInvoker)delegate { this.TextBoxSubscriber.Text = item + Environment.NewLine + this.TextBoxSubscriber.Text; });
        }
    }

}
