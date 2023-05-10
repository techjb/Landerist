﻿using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace landerist_library.Parse.ListingParser
{
    public class ChatGPT
    {

        // GPT-3.5-Turbo: 4096
        // GPT-4-8K: 8192
        // GPT-4-32K: 32768
        public static readonly int MAX_TOKENS = 4096;

        private readonly Conversation Conversation;

        public ChatGPT()
        {
            OpenAIAPI openAIAPI = new(Config.OPENAI_API_KEY);
            var chatRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0,
                //MaxTokens = 50,
            };
            Conversation = openAIAPI.Chat.CreateConversation(chatRequest);
            var systemMessage = GetSystemMessage();
            Conversation.AppendSystemMessage(systemMessage);
        }

        public string? GetResponse(string userInput)
        {
            Conversation.AppendUserInput(userInput);
            try
            {
                string response = Task.Run(async () => await Conversation.GetResponseFromChatbotAsync()).Result;
                return response;
            }
            catch
            {
                return null;
            }
        }

        private static string GetSystemMessage()
        {
            return
                "Proporciona una representación JSON que siga estrictamente este esquema:\n\n" +
                ListingResponseSchema.GetSchema() + "\n\n" +
                "Escribe null en los campos que falten.";
        }

        public static bool IsLengthAllowed(string request)
        {
            //https://github.com/dluc/openai-tools
            var systemMessage = GetSystemMessage();
            int systemTokens = GPT3Tokenizer.Encode(systemMessage).Count;

            int userTokens = GPT3Tokenizer.Encode(request).Count;

            int totalTokens = systemTokens + userTokens;
            return totalTokens < MAX_TOKENS;
        }
    }
}