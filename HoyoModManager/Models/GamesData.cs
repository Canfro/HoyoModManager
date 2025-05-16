using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HoyoModManager.Models;

public static class GamesData
{
    public static ObservableCollection<string> Games { get; } =
    [
        "Genshin Impact", "Honkai: Star Rail", "Zenless Zone Zero",
    ];
    
    public static ObservableCollection<string> GenshinNames { get; } =
    [
        "Aether", "Albedo", "Alhaitham", "Aloy", "Amber", "Arataki Itto", "Arlecchino", "Baizhu", "Barbara", "Beidou", "Bennet",
        "Candace", "Charlotte", "Chasca", "Chevreuse", "Chiori", "Chongyun", "Citlali", "Clorinde", "Collei", "Cyno", "Dehya",
        "Diluc", "Diona", "Dori", "Emilie", "Escoffier", "Eula", "Faruzan", "Fischl", "Freminet", "Furina", "Gaming", "Ganyu",
        "Gorou", "Hu Tao", "Iansan", "Ifa", "Jean", "Kachina", "Kaedehara Kazuha", "Kaeya", "Kamisato Ayaka", "Kamisato Ayato",
        "Kaveh", "Keqing", "Kinich", "Kirara", "Klee", "Kujou Sara", "Kuki Shinobu", "Lan Yan", "Layla", "Lisa", "Lumine", "Lynette",
        "Lyney", "Mavuika", "Mika", "Mona", "Mualani", "Nahida", "Navia", "Neuvillette", "Nilou", "Ningguang", "Noelle", "Ororon",
        "Qiqi", "Raiden Shogun", "Razor", "Rosaria", "Sangonomiya Kokomi", "Sayu", "Sethos", "Shenhe", "Shikanoin Heizou",
        "Sigewinne", "Sucrose", "Tartaglia", "Thoma", "Tighnari", "Varesa", "Venti", "Wanderer", "Wriothesley", "Xiangling",
        "Xianyun", "Xiao", "Xilonen", "Xingqiu", "Xinyan", "Yae Miko", "Yanfei", "Yaoyao", "Yelan", "Yoimiya", "Yumemizuki Mizuki",
        "Yun Jin", "Zhongli",
    ];

    public static ObservableCollection<string> StarRailNames { get; } =
    [
        "Acheron", "Aglaea", "Anaxa", "Arlan", "Argenti", "Asta", "Aventurine", "Bailu", "Black Swan", "Blade", "Boothill", "Bronya",
        "Caelus", "Castorice", "Clara", "Dan Heng", "Dan Heng - Imbibitor Lunae", "Dr. Ratio", "Feixiao", "Firefly", "Fu Xuan", 
        "Fugue", "Gallagher", "Gepard", "Guinaifen", "Hanya", "Herta", "Himeko", "Hook", "Huohuo", "Jade", "Jiaoqiu", "Jing Yuan",
        "Jingliu", "Kafka", "Lingsha", "Luocha", "Luka", "Lynx", "March 7th", "March 7th (hunt)", "Misha", "Moze", "Mydei", "Natasha",
        "Pela", "Qingque", "Rappa", "Robin", "Ruan Mei", "Sampo", "Seele", "Serval", "Silver Wolf", "Sparkle", "Stelle", "Sunday",
        "Sushang", "The Herta", "Tingyun", "Topaz and Numby", "Tribbie", "Welt", "Xueyi", "Yanqing", "Yukong", "Yunli",
    ];
    
    public static ObservableCollection<string> ZenlessNames { get; } =
    [
        "Anby", "Anby Soldier 0", "Anton", "Astra Yao", "Ben", "Billy", "Burnice", "Caesar", "Corin", "Ellen", "Evelyn", "Grace", 
        "Harumasa", "Jane Doe", "Koleda", "Lighter", "Lucy", "Lycaon", "Miyabi", "Nicole", "Nekomata", "Piper", "Pulchra", "Qingyi", 
        "Rina", "Seth", "Soldier 11", "Soukaku", "Trigger", "Vivian", "Yanagi", "Zhu Yuan",
    ];
}
