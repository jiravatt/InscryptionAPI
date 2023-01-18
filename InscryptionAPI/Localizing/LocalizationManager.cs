﻿using HarmonyLib;

namespace InscryptionAPI.Localizing;

public static class LocalizationManager
{
    public class CustomTranslation
    {
        public string PluginGUID;
        public Localization.Translation Translation;
    }
    
    public static List<CustomTranslation> CustomTranslations = new List<CustomTranslation>();
    public static Action<Language> OnLanguageLoaded = null;
    
    private static List<Language> AlreadyLoadedLanguages = new List<Language>();

    public static CustomTranslation New(string pluginGUID, string id, string englishString, string translatedString, Language language)
    {
        CustomTranslation customTranslation = new CustomTranslation();
        customTranslation.PluginGUID = pluginGUID;
        
        customTranslation.Translation = new Localization.Translation
        {
            id = id,
            englishString = englishString,
            englishStringFormatted = !string.IsNullOrEmpty(englishString) ? Localization.FormatString(englishString) : null,
            values = new Dictionary<Language, string>()
            {
                {language, translatedString}
            },
            femaleGenderValues = new Dictionary<Language, string>()
        };

        return Add(customTranslation);
    }

    public static CustomTranslation Add(CustomTranslation translation)
    {
        CustomTranslations.Add(translation);
        if (AlreadyLoadedLanguages.Count > 0)
        {
            InsertTranslation(translation);
        }
        return translation;
    }

    private static void InsertTranslation(CustomTranslation customTranslation)
    {
        Localization.Translation translation = null;
        if (!string.IsNullOrEmpty(customTranslation.Translation.englishStringFormatted))
        {
            translation = Localization.Translations.Find((a) => a.englishStringFormatted == customTranslation.Translation.englishStringFormatted);
        }
        else
        {
            translation = Localization.Translations.Find((a) => a.id == customTranslation.Translation.id);
        }

        if (translation == null)
        {
            translation = new Localization.Translation();
            translation.id = customTranslation.Translation.id; 
            translation.englishString = customTranslation.Translation.englishString; 
            translation.englishStringFormatted = customTranslation.Translation.englishStringFormatted; 
            translation.values = new Dictionary<Language, string>(); 
            translation.femaleGenderValues = new Dictionary<Language, string>(); 
            Localization.Translations.Add(translation);
        }
        
        // Update translations
        foreach (Language language in AlreadyLoadedLanguages)
        {
            if (customTranslation.Translation.values.TryGetValue(language, out string word))
            {
                translation.values[language] = word;
            }
            if (customTranslation.Translation.femaleGenderValues.TryGetValue(language, out string femaleWord))
            {
                translation.femaleGenderValues[language] = femaleWord;
            }
        }
    }

    [HarmonyPatch]
    internal static class Patches
    {
        [HarmonyPatch(typeof(Localization), nameof(Localization.ReadCSVFileIntoTranslationData))]
        [HarmonyPostfix]
        private static void ReadCSVFileIntoTranslationData(Language language)
        {
            if (!AlreadyLoadedLanguages.Contains(language))
            {
                AlreadyLoadedLanguages.Add(language);
            }
            foreach (CustomTranslation translation in CustomTranslations)
            {
                InsertTranslation(translation);
            }

            if (OnLanguageLoaded != null)
            {
                OnLanguageLoaded(language);
            }
        }
    }
}
