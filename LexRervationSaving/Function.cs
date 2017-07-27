using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.LexEvents;
using Amazon.DynamoDBv2.DocumentModel;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LexRervationSaving
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public LexResponse FunctionHandler(LexEvent e, ILambdaContext context)
        {
            try
            {
                LexResponse r = new LexResponse();

                Guid g = Guid.Empty;

                if (e.InvocationSource == "FulfillmentCodeHook")
                {
                    g = Guid.NewGuid();
                    using (AmazonDynamoDBClient client = new AmazonDynamoDBClient())
                    {

                        Table reservations = Table.LoadTable(client, "Lex_Reservation");
                        Document doc = new Document();

                        doc["ReservationId"] = g.ToString();
                        doc["ReservationDate"] = DateTime.Now.ToString("yyyy:MM:dd:HH:mm:ss");
                        if(e.CurrentIntent.Name== "BookHotel")
                        {
                            doc["Type"] = "Hotel";
                            doc["Location"] = e.CurrentIntent.Slots["Location"];
                            doc["CheckInDate"] = e.CurrentIntent.Slots["CheckInDate"];
                            doc["Nights"] = e.CurrentIntent.Slots["Nights"];
                            doc["RoomType"] = e.CurrentIntent.Slots["RoomType"];
                        }
                        if (e.CurrentIntent.Name == "BookCar")
                        {
                            doc["Type"] = "Car";
                            doc["PickupCity"] = e.CurrentIntent.Slots["Location"];
                            doc["PickupDate"] = e.CurrentIntent.Slots["CheckInDate"];
                            doc["ReturnDate"] = e.CurrentIntent.Slots["Nights"];
                            doc["AverageAge"] = e.CurrentIntent.Slots["RoomType"];
                            doc["CarType"] = e.CurrentIntent.Slots["CarType"];
                        }


                        Task<Document> t = reservations.PutItemAsync(doc);
                        t.Wait();
                        //context.Logger.Log("Saved successfully!!!");
                        //r.DialogAction.Type = "Close";
                        //r.SessionAttributes.Add("RervervationGuid", g.ToString());

                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                context.Logger.Log(ex.ToString());
                throw;
            }
        }
    }
}
