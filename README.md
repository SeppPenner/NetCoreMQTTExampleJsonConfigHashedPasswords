NetCoreMQTTExampleJsonConfigHashedPasswords
====================================

NetCoreMQTTExampleJsonConfigHashedPasswords is a project to check user credentials and topic restrictions from [MQTTnet](https://github.com/chkr1011/MQTTnet) from a json config file using hashed user passwords.
The project was written and tested in .NetCore 3.0.

[![Build status](https://ci.appveyor.com/api/projects/status/ngv3j94bd6l0klba?svg=true)](https://ci.appveyor.com/project/SeppPenner/netcoremqttexamplejsonconfighashedpasswords)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords.svg)](https://github.com/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords.svg)](https://github.com/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords.svg)](https://github.com/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords/stargazers)
[![GitHub license](https://img.shields.io/badge/license-AGPL-blue.svg)](https://raw.githubusercontent.com/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords/master/License.txt)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords/badge.svg)](https://snyk.io/test/github/SeppPenner/NetCoreMQTTExampleJsonConfigHashedPasswords)

## How to use this project:
1. Create your password hashes for each user using the `CreateHashes` project.
2. Add the passwords and other data to the config.json file.

## JSON configuration (Adjust this to your needs):

Users can be defined in two different ways (The ways can be combined making sure that the client ids and client id prefixes need to be distinct for all of them, of course):

### Exact definition (Matching exactly one client id):

```json
{
  "Port": 8883,
  "Users": [
    {
      "UserName": "Hans",
      "ClientId": "Hans",
      "Password": "AQAAAAEAACcQAAAAEKsbxxvBm/peZayW9Qmo9Rd1tRF4SLX4CQ6pNSrDSmCMWYf7o8Iy2pZCTA+No0fB8Q==",
      "SubscriptionTopicLists": {
        "BlacklistTopics": [
          "a",
          "b/+",
          "c/#"
        ],
        "WhitelistTopics": [
          "d",
          "e/+",
          "f/#"
        ]
      },
      "PublishTopicLists": {
        "BlacklistTopics": [
          "a",
          "b/+",
          "c/#"
        ],
        "WhitelistTopics": [
          "d",
          "e/+",
          "f/#"
        ]
      }
    }
  ]
}
```

### Various definition (Matching multiple client ids for one username and password combination):

```json
{
  "Port": 8883,
  "Users": [
    {
      "UserName": "Hans2",
      "Password": "AQAAAAEAACcQAAAAECcnkwU+LImyVorjCCNzpTGgYOjVxFd+i/PW3MyU2sws80uPkPrppb+AXnvaxVI/0Q==",
      "ClientIdPrefix": "Test_",
      "SubscriptionTopicLists": {
        "BlacklistTopics": [
          "g",
          "h/+",
          "i/#"
        ],
        "WhitelistTopics": [
          "j",
          "k/+",
          "l/#"
        ]
      },
      "PublishTopicLists": {
        "BlacklistTopics": [
          "g",
          "h/+",
          "i/#"
        ],
        "WhitelistTopics": [
          "j",
          "k/+",
          "l/#"
        ]
      }
    }
  ]
}
```

## Attention:
* Only the following [UTF-8](https://www.utf8-chartable.de/unicode-utf8-table.pl) chars are supported for topics:

|Unicode code point|character|UTF-8(hex.)|Name|
|-|-|-|-|
|U+0021|`!`|21|EXCLAMATION MARK|
|U+0022|`"`|22|QUOTATION MARK|
|U+0023|`#`|23|NUMBER SIGN|
|U+0024|`$`|24|DOLLAR SIGN|
|U+0025|`%`|25|PERCENT SIGN|
|U+0026|`&`|26|AMPERSAND|
|U+0027|`'`|27|APOSTROPHE|
|U+0028|`(`|28|LEFT PARENTHESIS|
|U+0029|`)`|29|RIGHT PARENTHESIS|
|U+002A|`*`|2a|ASTERISK|
|U+002B|`+`|2b|PLUS SIGN|
|U+002C|`,`|2c|COMMA|
|U+002D|`-`|2d|HYPHEN-MINUS|
|U+002E|`.`|2e|FULL STOP|
|U+002F|`/`|2f|SOLIDUS|
|U+0030|`0`|30|DIGIT ZERO|
|U+0031|`1`|31|DIGIT ONE|
|U+0032|`2`|32|DIGIT TWO|
|U+0033|`3`|33|DIGIT THREE|
|U+0034|`4`|34|DIGIT FOUR|
|U+0035|`5`|35|DIGIT FIVE|
|U+0036|`6`|36|DIGIT SIX|
|U+0037|`7`|37|DIGIT SEVEN|
|U+0038|`8`|38|DIGIT EIGHT|
|U+0039|`9`|39|DIGIT NINE|
|U+003A|`:`|3a|COLON|
|U+003B|`;`|3b|SEMICOLON|
|U+003C|`<`|3c|LESS-THAN SIGN|
|U+003D|`=`|3d|EQUALS SIGN|
|U+003E|`>`|3e|GREATER-THAN SIGN|
|U+003F|`?`|3f|QUESTION MARK|
|U+0040|`@`|40|COMMERCIAL AT|
|U+0041|`A`|41|LATIN CAPITAL LETTER A|
|U+0042|`B`|42|LATIN CAPITAL LETTER B|
|U+0043|`C`|43|LATIN CAPITAL LETTER C|
|U+0044|`D`|44|LATIN CAPITAL LETTER D|
|U+0045|`E`|45|LATIN CAPITAL LETTER E|
|U+0046|`F`|46|LATIN CAPITAL LETTER F|
|U+0047|`G`|47|LATIN CAPITAL LETTER G|
|U+0048|`H`|48|LATIN CAPITAL LETTER H|
|U+0049|`I`|49|LATIN CAPITAL LETTER I|
|U+004A|`J`|4a|LATIN CAPITAL LETTER J|
|U+004B|`K`|4b|LATIN CAPITAL LETTER K|
|U+004C|`L`|4c|LATIN CAPITAL LETTER L|
|U+004D|`M`|4d|LATIN CAPITAL LETTER M|
|U+004E|`N`|4e|LATIN CAPITAL LETTER N|
|U+004F|`O`|4f|LATIN CAPITAL LETTER O|
|U+0050|`P`|50|LATIN CAPITAL LETTER P|
|U+0051|`Q`|51|LATIN CAPITAL LETTER Q|
|U+0052|`R`|52|LATIN CAPITAL LETTER R|
|U+0053|`S`|53|LATIN CAPITAL LETTER S|
|U+0054|`T`|54|LATIN CAPITAL LETTER T|
|U+0055|`U`|55|LATIN CAPITAL LETTER U|
|U+0056|`V`|56|LATIN CAPITAL LETTER V|
|U+0057|`W`|57|LATIN CAPITAL LETTER W|
|U+0058|`X`|58|LATIN CAPITAL LETTER X|
|U+0059|`Y`|59|LATIN CAPITAL LETTER Y|
|U+005A|`Z`|5a|LATIN CAPITAL LETTER Z|
|U+005B|`[`|5b|LEFT SQUARE BRACKET|
|U+005C|`\`|5c|REVERSE SOLIDUS|
|U+005D|`]`|5d|RIGHT SQUARE BRACKET|
|U+005E|`^`|5e|CIRCUMFLEX ACCENT|
|U+005F|`_`|5f|LOW LINE|
|U+0060|<code>`</code>|60|GRAVE ACCENT|
|U+0061|`a`|61|LATIN SMALL LETTER A|
|U+0062|`b`|62|LATIN SMALL LETTER B|
|U+0063|`c`|63|LATIN SMALL LETTER C|
|U+0064|`d`|64|LATIN SMALL LETTER D|
|U+0065|`e`|65|LATIN SMALL LETTER E|
|U+0066|`f`|66|LATIN SMALL LETTER F|
|U+0067|`g`|67|LATIN SMALL LETTER G|
|U+0068|`h`|68|LATIN SMALL LETTER H|
|U+0069|`i`|69|LATIN SMALL LETTER I|
|U+006A|`j`|6a|LATIN SMALL LETTER J|
|U+006B|`k`|6b|LATIN SMALL LETTER K|
|U+006C|`l`|6c|LATIN SMALL LETTER L|
|U+006D|`m`|6d|LATIN SMALL LETTER M|
|U+006E|`n`|6e|LATIN SMALL LETTER N|
|U+006F|`o`|6f|LATIN SMALL LETTER O|
|U+0070|`p`|70|LATIN SMALL LETTER P|
|U+0071|`q`|71|LATIN SMALL LETTER Q|
|U+0072|`r`|72|LATIN SMALL LETTER R|
|U+0073|`s`|73|LATIN SMALL LETTER S|
|U+0074|`t`|74|LATIN SMALL LETTER T|
|U+0075|`u`|75|LATIN SMALL LETTER U|
|U+0076|`v`|76|LATIN SMALL LETTER V|
|U+0077|`w`|77|LATIN SMALL LETTER W|
|U+0078|`x`|78|LATIN SMALL LETTER X|
|U+0079|`y`|79|LATIN SMALL LETTER Y|
|U+007A|`z`|7a|LATIN SMALL LETTER Z|
|U+007B|`{`|7b|LEFT CURLY BRACKET|
|U+007C|`|`|7c|VERTICAL LINE|
|U+007D|`}`|7d|RIGHT CURLY BRACKET|
|U+007E|`~`|7e|TILDE|
|U+00A1|`¡`|c2 a1|INVERTED EXCLAMATION MARK|
|U+00A2|`¢`|c2 a2|CENT SIGN|
|U+00A3|`£`|c2 a3|POUND SIGN|
|U+00A4|`¤`|c2 a4|CURRENCY SIGN|
|U+00A5|`¥`|c2 a5|YEN SIGN|
|U+00A6|`¦`|c2 a6|BROKEN BAR|
|U+00A7|`§`|c2 a7|SECTION SIGN|
|U+00A8|`¨`|c2 a8|DIAERESIS|
|U+00A9|`©`|c2 a9|COPYRIGHT SIGN|
|U+00AA|`ª`|c2 aa|FEMININE ORDINAL INDICATOR|
|U+00AB|`«`|c2 ab|LEFT-POINTING DOUBLE ANGLE QUOTATION MARK|
|U+00AC|`¬`|c2 ac|NOT SIGN|
|U+00AE|`®`|c2 ae|REGISTERED SIGN|
|U+00AF|`¯`|c2 af|MACRON|
|U+00B0|`°`|c2 b0|DEGREE SIGN|
|U+00B1|`±`|c2 b1|PLUS-MINUS SIGN|
|U+00B2|`²`|c2 b2|SUPERSCRIPT TWO|
|U+00B3|`³`|c2 b3|SUPERSCRIPT THREE|
|U+00B4|`´`|c2 b4|ACUTE ACCENT|
|U+00B5|`µ`|c2 b5|MICRO SIGN|
|U+00B6|`¶`|c2 b6|PILCROW SIGN|
|U+00B7|`·`|c2 b7|MIDDLE DOT|
|U+00B8|`¸`|c2 b8|CEDILLA|
|U+00B9|`¹`|c2 b9|SUPERSCRIPT ONE|
|U+00BA|`º`|c2 ba|MASCULINE ORDINAL INDICATOR|
|U+00BB|`»`|c2 bb|RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK|
|U+00BC|`¼`|c2 bc|VULGAR FRACTION ONE QUARTER|
|U+00BD|`½`|c2 bd|VULGAR FRACTION ONE HALF|
|U+00BE|`¾`|c2 be|VULGAR FRACTION THREE QUARTERS|
|U+00BF|`¿`|c2 bf|INVERTED QUESTION MARK|
|U+00C0|`À`|c3 80|LATIN CAPITAL LETTER A WITH GRAVE|
|U+00C1|`Á`|c3 81|LATIN CAPITAL LETTER A WITH ACUTE|
|U+00C2|`Â`|c3 82|LATIN CAPITAL LETTER A WITH CIRCUMFLEX|
|U+00C3|`Ã`|c3 83|LATIN CAPITAL LETTER A WITH TILDE|
|U+00C4|`Ä`|c3 84|LATIN CAPITAL LETTER A WITH DIAERESIS|
|U+00C5|`Å`|c3 85|LATIN CAPITAL LETTER A WITH RING ABOVE|
|U+00C6|`Æ`|c3 86|LATIN CAPITAL LETTER AE|
|U+00C7|`Ç`|c3 87|LATIN CAPITAL LETTER C WITH CEDILLA|
|U+00C8|`È`|c3 88|LATIN CAPITAL LETTER E WITH GRAVE|
|U+00C9|`É`|c3 89|LATIN CAPITAL LETTER E WITH ACUTE|
|U+00CA|`Ê`|c3 8a|LATIN CAPITAL LETTER E WITH CIRCUMFLEX|
|U+00CB|`Ë`|c3 8b|LATIN CAPITAL LETTER E WITH DIAERESIS|
|U+00CC|`Ì`|c3 8c|LATIN CAPITAL LETTER I WITH GRAVE|
|U+00CD|`Í`|c3 8d|LATIN CAPITAL LETTER I WITH ACUTE|
|U+00CE|`Î`|c3 8e|LATIN CAPITAL LETTER I WITH CIRCUMFLEX|
|U+00CF|`Ï`|c3 8f|LATIN CAPITAL LETTER I WITH DIAERESIS|
|U+00D0|`Ð`|c3 90|LATIN CAPITAL LETTER ETH|
|U+00D1|`Ñ`|c3 91|LATIN CAPITAL LETTER N WITH TILDE|
|U+00D2|`Ò`|c3 92|LATIN CAPITAL LETTER O WITH GRAVE|
|U+00D3|`Ó`|c3 93|LATIN CAPITAL LETTER O WITH ACUTE|
|U+00D4|`Ô`|c3 94|LATIN CAPITAL LETTER O WITH CIRCUMFLEX|
|U+00D5|`Õ`|c3 95|LATIN CAPITAL LETTER O WITH TILDE|
|U+00D6|`Ö`|c3 96|LATIN CAPITAL LETTER O WITH DIAERESIS|
|U+00D7|`×`|c3 97|MULTIPLICATION SIGN|
|U+00D8|`Ø`|c3 98|LATIN CAPITAL LETTER O WITH STROKE|
|U+00D9|`Ù`|c3 99|LATIN CAPITAL LETTER U WITH GRAVE|
|U+00DA|`Ú`|c3 9a|LATIN CAPITAL LETTER U WITH ACUTE|
|U+00DB|`Û`|c3 9b|LATIN CAPITAL LETTER U WITH CIRCUMFLEX|
|U+00DC|`Ü`|c3 9c|LATIN CAPITAL LETTER U WITH DIAERESIS|
|U+00DD|`Ý`|c3 9d|LATIN CAPITAL LETTER Y WITH ACUTE|
|U+00DE|`Þ`|c3 9e|LATIN CAPITAL LETTER THORN|
|U+00DF|`ß`|c3 9f|LATIN SMALL LETTER SHARP S|
|U+00E0|`à`|c3 a0|LATIN SMALL LETTER A WITH GRAVE|
|U+00E1|`á`|c3 a1|LATIN SMALL LETTER A WITH ACUTE|
|U+00E2|`â`|c3 a2|LATIN SMALL LETTER A WITH CIRCUMFLEX|
|U+00E3|`ã`|c3 a3|LATIN SMALL LETTER A WITH TILDE|
|U+00E4|`ä`|c3 a4|LATIN SMALL LETTER A WITH DIAERESIS|
|U+00E5|`å`|c3 a5|LATIN SMALL LETTER A WITH RING ABOVE|
|U+00E6|`æ`|c3 a6|LATIN SMALL LETTER AE|
|U+00E7|`ç`|c3 a7|LATIN SMALL LETTER C WITH CEDILLA|
|U+00E8|`è`|c3 a8|LATIN SMALL LETTER E WITH GRAVE|
|U+00E9|`é`|c3 a9|LATIN SMALL LETTER E WITH ACUTE|
|U+00EA|`ê`|c3 aa|LATIN SMALL LETTER E WITH CIRCUMFLEX|
|U+00EB|`ë`|c3 ab|LATIN SMALL LETTER E WITH DIAERESIS|
|U+00EC|`ì`|c3 ac|LATIN SMALL LETTER I WITH GRAVE|
|U+00ED|`í`|c3 ad|LATIN SMALL LETTER I WITH ACUTE|
|U+00EE|`î`|c3 ae|LATIN SMALL LETTER I WITH CIRCUMFLEX|
|U+00EF|`ï`|c3 af|LATIN SMALL LETTER I WITH DIAERESIS|
|U+00F0|`ð`|c3 b0|LATIN SMALL LETTER ETH|
|U+00F1|`ñ`|c3 b1|LATIN SMALL LETTER N WITH TILDE|
|U+00F2|`ò`|c3 b2|LATIN SMALL LETTER O WITH GRAVE|
|U+00F3|`ó`|c3 b3|LATIN SMALL LETTER O WITH ACUTE|
|U+00F4|`ô`|c3 b4|LATIN SMALL LETTER O WITH CIRCUMFLEX|
|U+00F5|`õ`|c3 b5|LATIN SMALL LETTER O WITH TILDE|
|U+00F6|`ö`|c3 b6|LATIN SMALL LETTER O WITH DIAERESIS|
|U+00F7|`÷`|c3 b7|DIVISION SIGN|
|U+00F8|`ø`|c3 b8|LATIN SMALL LETTER O WITH STROKE|
|U+00F9|`ù`|c3 b9|LATIN SMALL LETTER U WITH GRAVE|
|U+00FA|`ú`|c3 ba|LATIN SMALL LETTER U WITH ACUTE|
|U+00FB|`û`|c3 bb|LATIN SMALL LETTER U WITH CIRCUMFLEX|
|U+00FC|`ü`|c3 bc|LATIN SMALL LETTER U WITH DIAERESIS|
|U+00FD|`ý`|c3 bd|LATIN SMALL LETTER Y WITH ACUTE|
|U+00FE|`þ`|c3 be|LATIN SMALL LETTER THORN|
|U+00FF|`ÿ`|c3 bf|LATIN SMALL LETTER Y WITH DIAERESIS|

## Password and Hash examples:

|Password|Hash|
|-|-|
|Test|AQAAAAEAACcQAAAAEKsbxxvBm/peZayW9Qmo9Rd1tRF4SLX4CQ6pNSrDSmCMWYf7o8Iy2pZCTA+No0fB8Q==|
|Test|AQAAAAEAACcQAAAAECcnkwU+LImyVorjCCNzpTGgYOjVxFd+i/PW3MyU2sws80uPkPrppb+AXnvaxVI/0Q==|

## Create an openssl certificate:
```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem
```

An example certificate is in the folder. Password for all is `test`.

Change history
--------------

* **Version 1.0.2.0 (2019-09-29)** : Updated to .NetCore 3.0, updated nuget packages, fixed code style.
* **Version 1.0.1.0 (2019-08-22)** : Updated MQTTnet to 3.0.8.
* **Version 1.0.0.0 (2019-07-14)** : 1.0 release.