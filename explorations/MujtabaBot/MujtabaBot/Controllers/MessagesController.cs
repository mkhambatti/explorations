using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Text;

namespace MujtabaBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        //public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        //{
        //    if (activity.Type == ActivityTypes.Message)
        //    {
        //        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
        //        // calculate something for us to return
        //        int length = (activity.Text ?? string.Empty).Length;

        //        // return our reply to the user
        //        Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");

        //        await connector.Conversations.ReplyToActivityAsync(reply);
        //    }
        //    else
        //    {
        //        HandleSystemMessage(activity);
        //    }
        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    return response;
        //}
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            // Global values
            bool boolAskedForUserName = false;
            string strUserName = "";

            if (activity.Type == ActivityTypes.Message)
            {
                // Get any saved values
                StateClient sc = activity.GetStateClient();

                BotData userData = sc.BotState.GetPrivateConversationData(
                    activity.ChannelId, activity.Conversation.Id, activity.From.Id);

                boolAskedForUserName = userData.GetProperty<bool>("AskedForUserName");
                strUserName = userData.GetProperty<string>("UserName") ?? "";

                // Create text for a reply message   
                StringBuilder strReplyMessage = new StringBuilder();

                if (boolAskedForUserName == false) // Never asked for name
                {
                    strReplyMessage.Append($"Hello, I am **MujtabaBot**");
                    strReplyMessage.Append($"{Environment.NewLine}");
                    strReplyMessage.Append($"You can say anything");
                    strReplyMessage.Append($"{Environment.NewLine}");
                    strReplyMessage.Append($"to me and I will repeat it back... really try me :)");
                    strReplyMessage.Append($"{Environment.NewLine}{Environment.NewLine}");
                    strReplyMessage.Append($"What is your name?");

                    // Set BotUserData
                    userData.SetProperty("AskedForUserName", true);
                }

                else // Have asked for name
                {
                    if (strUserName == "") // Name was never provided
                    {
                        // If we have asked for a username but it has not been set
                        // the current response is the user name
                        strReplyMessage.Append($"Hello {activity.Text}!");

                        // Set BotUserData
                        userData.SetProperty("UserName", activity.Text);
                    }

                    else // Name was provided
                    {
                        strReplyMessage.Append($"{strUserName}, You said: {activity.Text}");
                    }
                }

                // Save BotUserData
                sc.BotState.SetPrivateConversationData(
                    activity.ChannelId, activity.Conversation.Id, activity.From.Id, userData);

                // Create a reply message
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity replyMessage = activity.CreateReply(strReplyMessage.ToString());

                await connector.Conversations.ReplyToActivityAsync(replyMessage);
            }

            else
            {
                HandleSystemMessage(activity);
            }

            // Return response
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Get BotUserData
                StateClient sc = message.GetStateClient();
                BotData userData = sc.BotState.GetPrivateConversationData(
                    message.ChannelId, message.Conversation.Id, message.From.Id);

                // Set BotUserData
                userData.SetProperty("UserName", "");
                userData.SetProperty("AskedForUserName", false);

                // Save BotUserData
                sc.BotState.SetPrivateConversationData(
                    message.ChannelId, message.Conversation.Id, message.From.Id, userData);

                // Create a reply message
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                Activity replyMessage = message.CreateReply("MujtabaBot has been successfully reset.");
                return replyMessage;
            }

            //if (message.Type == ActivityTypes.DeleteUserData)
            //{
            //    // Implement user deletion here
            //    // If we handle user deletion, return a real message
            //}
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}