from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI


def common_tasks():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.do_common_tasks(tf.get_version_of_project())#codeunit-version should alsways be the same as project-version
    tf.tfcps_Tools_General.generate_api_client_from_dependent_codeunit(tf.get_codeunit_folder(),"OpenIMSBackend","src/app/generated/open-ims-backend", "typescript-angular",tf.use_cache(),["apis","models","supportingFiles"])
    tf.organize_translations([
        "af",  # Afrikaans
        "ar",  # Arabic
        "ar-DZ",  # Arabic (Algeria)
        "ar-EG",  # Arabic (Egypt)
        "ar-LY",  # Arabic (Libya)
        "ar-MA",  # Arabic (Morocco)
        "ar-SD",  # Arabic (Sudan)
        "ar-TN",  # Arabic (Tunisia)
        "bn",  # Bengali
        "cs",  # Czech
        "da",  # Danish
        "de",  # German
        "de-AT",  # German (Austria)
        "de-CH",  # German (Switzerland)
        "el",  # Greek
        "en",  # English
        "en-GB",  # English (United Kingdom)
        "es",  # Spanish
        "es-AR",  # Spanish (Argentina)
        "es-CL",  # Spanish (Chile)
        "es-CO",  # Spanish (Colombia)
        "es-CR",  # Spanish (Costa Rica)
        "es-DO",  # Spanish (Dominican Republic)
        "es-EC",  # Spanish (Ecuador)
        "es-MX",  # Spanish (Mexico)
        "es-VE",  # Spanish (Venezuela)
        "et",  # Estonian
        "fa",  # Persian
        "fi",  # Finnish
        "fr",  # French
        "ga",  # Irish
        "he",  # Hebrew
        "hi",  # Hindi
        "hr",  # Croatian
        "hu",  # Hungarian
        "id",  # Indonesian
        "is",  # Icelandic
        "it",  # Italian
        "ja",  # Japanese
        "ka",  # Georgian
        "kk",  # Kazakh
        "km",  # Khmer
        "ky",  # Kyrgyz
        "ko",  # Korean
        "lo",  # Lao
        "lv",  # Latvian
        "lt",  # Lithuanian
        "ms",  # Malay
        "ms-SG",  # Malay (Singapore)
        "mn",  # Mongolian
        "my",  # Burmese
        "nb",  # Norwegian Bokmål
        "nl",  # Dutch
        "ph",  # Filipino
        "pl",  # Polish
        "pt",  # Portuguese
        "pt-BR",  # Portuguese (Brazil)
        "ro",  # Romanian
        "ru",  # Russian
        "sv",  # Swedish
        "th",  # Thai
        "tk",  # Turkmen
        "tl",  # Tagalog
        "tr",  # Turkish
        "uk",  # Ukrainian
        "ur",  # Urdu
        "vi",  # Vietnamese
        "zh",  # Chinese
    ])
     
if __name__ == "__main__":
    common_tasks()
 