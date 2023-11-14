﻿using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;
using System.Security.Cryptography;
using LLama.Common;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.TextCompletion;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace MattEland.BatComputer.Kernel;

public class MePlugin
{
    private readonly KernelSettings _settings;
    private readonly AppKernel _kernel;

    public MePlugin(KernelSettings settings, AppKernel kernel)
    {
        _settings = settings;
        _kernel = kernel;
    }

    [SKFunction, Description("Gets the user's current zip code")]
    public string GetZipCode()
    {
        return "43081"; // TODO: Read from settings
    }

    [SKFunction, Description("Gets the user's current city")]
    public string GetCity()
    {
        return "Columbus, Ohio"; // TODO: Read from settings
    }

    [SKFunction, Description("Gets the user's clothing preferences based on the weather and rain")]
    public string GetClothingPreferences()
    {
        return "The user prefers to wear a sun hat if it is warm. If it is rainy, the user prefers a waxed rain hat. For moderate temperatures, the user prefers a leather hat if it might rain and a suede leather hat if it won't rain. If it is cold and cloudy the user prefers a wool ivy hat. If it is cold and sunny the user prefers a wool fedora.";
    }

    [SKFunction, Description("Generates a recommendation for clothing to wear given weather conditions and user preferences")]
    public async Task<string> GetClothingRecommendation([Description("The user's preferences")] string clothingPreferences, [Description("A weather summary")] string weather, [Description("The user's original message")] string originalMessage)
    {
        string command = $"{ChatPlugin.SystemText}. Given the following user preferences: {clothingPreferences} and weather summary {weather} provide a recommendation based on the user's message: {originalMessage}. Don't say it is sunny if it is night.";

        return await _kernel.GetPromptedReplyAsync(command);
    }

}
