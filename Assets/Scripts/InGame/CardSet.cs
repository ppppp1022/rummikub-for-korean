using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class CardSet : MonoBehaviour
{
    //초성
    private static readonly char[] Chosung = {
        '\u1100', '\u1101', '\u1102', '\u1103', '\u1104', '\u1105', '\u1106', '\u1107',
        '\u1108', '\u1109', '\u110A', '\u110B', '\u110C', '\u110D', '\u110E', '\u110F',
        '\u1110', '\u1111', '\u1112'
    };
    private static string[][] Word_Chosung = {
        new string[] { "ㄱ" },
        new string[] { "ㄱ", "ㄱ" },
        new string[] { "ㄴ" },
        new string[] { "ㄷ" },
        new string[] { "ㄷ", "ㄷ" },
        new string[] { "ㄹ" },
        new string[] { "ㅁ" },
        new string[] { "ㅂ" },
        new string[] { "ㅂ", "ㅂ" },
        new string[] { "ㅅ" },
        new string[] { "ㅅ", "ㅅ" },
        new string[] { "ㅇ" },
        new string[] { "ㅈ" },
        new string[] { "ㅈ", "ㅈ" },
        new string[] { "ㅊ" },
        new string[] { "ㅋ" },
        new string[] { "ㅌ" },
        new string[] { "ㅍ" },
        new string[] { "ㅎ" }
    };

    //중성
    private static readonly char[] Jungsung = {
        '\u1161', '\u1162', '\u1163', '\u1164', '\u1165', '\u1166', '\u1167', '\u1168',
        '\u1169', '\u116A', '\u116B', '\u116C', '\u116D', '\u116E', '\u116F', '\u1170',
        '\u1171', '\u1172', '\u1173', '\u1174', '\u1175'
    };
    private static string[][] Word_Jungsung = {
        new string[] { "ㅏ" },
        new string[] { "ㅏ", "ㅣ" },
        new string[] { "ㅑ" },
        new string[] { "ㅑ","ㅣ" },
        new string[] { "ㅓ" },
        new string[] { "ㅓ","ㅣ" },
        new string[] { "ㅕ" },
        new string[] { "ㅕ","ㅣ" },
        new string[] { "ㅗ" },
        new string[] { "ㅗ", "ㅏ"},
        new string[] {"ㅗ",  "ㅏ", "ㅣ" },
        new string[] {"ㅗ",  "ㅣ"},
        new string[] { "ㅛ" },
        new string[] { "ㅜ" },
        new string[] { "ㅜ", "ㅓ"},
        new string[] { "ㅜ", "ㅓ","ㅣ" },
        new string[] { "ㅜ" , "ㅣ"},
        new string[] { "ㅠ" },
        new string[] { "ㅡ" },
        new string[] { "ㅡ", "ㅣ" },
        new string[] { "ㅣ" }
    };

    //종성
    private static readonly char[] Jongsung = {
        '\u0000', '\u11A8', '\u11A9', '\u11AA', '\u11AB', '\u11AC', '\u11AD', '\u11AE',
        '\u11AF', '\u11B0', '\u11B1', '\u11B2', '\u11B3', '\u11B4', '\u11B5', '\u11B6',
        '\u11B7', '\u11B8', '\u11B9', '\u11BA', '\u11BB', '\u11BC', '\u11BD', '\u11BE',
        '\u11BF', '\u11C0', '\u11C1', '\u11C2'
    };
    private static string[][] Word_Jongsung = {
        new string[] {" "},
        new string[] { "ㄱ" },
        new string[] { "ㄱ", "ㄱ" },
        new string[] { "ㄱ", "ㅅ" },
        new string[] { "ㄴ" },
        new string[] { "ㄴ", "ㅈ" },
        new string[] { "ㄴ", "ㅎ" },
        new string[] { "ㄷ" },
        new string[] { "ㄹ" },
        new string[] { "ㄹ", "ㄱ" },
        new string[] { "ㄹ", "ㅁ" },
        new string[] { "ㄹ", "ㅂ" },
        new string[] { "ㄹ", "ㅅ" },
        new string[] { "ㄹ", "ㅌ" },
        new string[] { "ㄹ", "ㅍ" },
        new string[] { "ㄹ", "ㅎ" },
        new string[] { "ㅁ" },
        new string[] { "ㅂ" },
        new string[] { "ㅂ", "ㅅ" },
        new string[] { "ㅅ" },
        new string[] { "ㅅ", "ㅅ" },
        new string[] { "ㅇ" },
        new string[] { "ㅈ" },
        new string[] { "ㅊ" },
        new string[] { "ㅋ" },
        new string[] { "ㅌ" },
        new string[] { "ㅍ" },
        new string[] { "ㅎ" }
    };

    // 유니코드 한글 음절의 시작점
    private const int HangulBase = 0xAC00;
    private const int ChosungBase = 21 * 28;
    private const int JungsungBase = 28;
    // 네이버 백과사전 로그인시 필요한 client id와 pw
    public string clientID = "id";
    public string clientPW = "pw";

    // 네이버 json형식식
    [System.Serializable]
    public class NaverSearchResult
    {
        public string lastBuildDate;
        public int total;
        public int start;
        public int display;
        public List<NaverItem> items;  // items가 배열이므로 List 또는 배열로 선언
    }

    [System.Serializable]
    public class NaverItem
    {
        public string title;
        public string link;
        public string description;
        public string thumbnail;
    }
    private BatchBoard batchBoard;
    public int CardSetID = 0;
    public List<GameObject> FixedCardObj = new List<GameObject>();  //fixed된 카드의 gameobject를 저장 
    public List<int> wordList = new List<int>();
    public int[] Pos = new int[2] { 0, 0 };
    public bool IsValid = false;
    public bool IsSyllable = false;
    public string FinalWords;
    private string BackUpFinalWords;
    private void Awake()
    {
        batchBoard = GameObject.Find("BatchBoard").GetComponent<BatchBoard>();
    }
    public void HesitateParent()
    {
        foreach (Card card in GetComponentsInChildren<Card>())
        {
            //Debug.Log("Hesitate");
            card.transform.SetParent(transform.parent);
        }
        if (FixedCardObj.Count == 0)
        {
            Destroy(transform.gameObject);
        }
        else
        {
            transform.SetParent(null);
        }
    }
    public void SetOriginParent()
    {
        for (int i = 0; i < FixedCardObj.Count; i++)
        {
            if (FixedCardObj[i])
            {
                //Debug.Log("FixedCardObj.Count: " + FixedCardObj.Count);
                Card card = FixedCardObj[i].GetComponent<Card>();
                CardCenter cardCenter = card.transform.GetComponentInChildren<CardCenter>();

                batchBoard.BatchCardObj[cardCenter.LastBatchPosIndex] = null;
                batchBoard.BatchBaseObj[cardCenter.LastBatchPosIndex].GetComponent<BoxCollider2D>().enabled = true;

                cardCenter.LastBaseObj = batchBoard.BatchBaseObj[card.TheRealLastPosIndex];
                cardCenter.LastBatchPosIndex = card.TheRealLastPosIndex;
                card.transform.position = batchBoard.BatchBaseObj[card.TheRealLastPosIndex].transform.position;
                card.TheTempLastPosIndex = card.TheRealLastPosIndex;
                card.PastBaseObjIndex = card.TheRealLastPosIndex;

                batchBoard.BatchCardObj[card.TheRealLastPosIndex] = card.transform.gameObject;

                BoxCollider2D boxCollider2D = batchBoard.BatchBaseObj[card.TheRealLastPosIndex].GetComponent<BoxCollider2D>();

                boxCollider2D.enabled = false;

                FixedCardObj[i].transform.SetParent(transform);
                IsValid = true;
                IsSyllable = true;
                FinalWords = BackUpFinalWords;
            }
            else
            {
                FixedCardObj.RemoveAt(i);
                --i;
            }
        }
    }
    public void FixingTheCard()
    {
        MakeSyllable();
        FixedCardObj = new List<GameObject>();
        foreach (Card card in GetComponentsInChildren<Card>())
        {
            card.gameObject.tag = "FixedCard";
            CardCenter cardCenter = card.GetComponentInChildren<CardCenter>();
            card.TheRealLastPosIndex = cardCenter.LastBatchPosIndex;
            card.TheTempLastPosIndex = cardCenter.LastBatchPosIndex;
            card.FixedCardSetID = CardSetID;
            //Debug.Log("adding Fixed card");
            FixedCardObj.Add(card.gameObject);
        }
        BackUpFinalWords = FinalWords;
    }
    public void CheckRealWord() //글자가 실제 존재하는 글자인지
    {
        MakeSyllable();
        if (!IsSyllable)
        {
            Debug.Log("It can not be word!");
            GameObject.Find("ButtonListener").GetComponent<ManageSoundInGameRule>().PlaySound(1);
        }
        else
        {
            //Debug.Log("checking");
            string url = $"https://openapi.naver.com/v1/search/encyc.json?query={FinalWords}&display=5&start=1&sort=sim";
            StartCoroutine(GetRequest(url));
        }
    }
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.SetRequestHeader("X-Naver-Client-Id", clientID);
            webRequest.SetRequestHeader("X-Naver-Client-Secret", clientPW);
            yield return webRequest.SendWebRequest();

            string result = webRequest.downloadHandler.text;
            //Debug.Log(result);
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    IsValid = false;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    IsValid = false;
                    break;
                case UnityWebRequest.Result.Success:

                    NaverSearchResult resultData = JsonUtility.FromJson<NaverSearchResult>(result);
                    List<string> titleList = new List<string>();
                    foreach (NaverItem item in resultData.items)
                    {
                        titleList.Add(item.title);
                    }
                    string[] titleArray = titleList.ToArray();
                    for (int i = 0; i < titleArray.Length; i++)
                    {
                        titleArray[i] = RemoveHtmlTags(titleArray[i]);
                    }

                    foreach (var t in titleArray)
                    {
                        if (t == FinalWords)
                        {
                            Debug.Log(t);
                            IsValid = true;

                            break;
                        }
                    }
                    break;
            }
            GameObject.Find("ButtonListener").GetComponent<CreateCard>().CheckAll++;
        }
    }
    private static string RemoveHtmlTags(string source)  // html 태그 제거
    {
        // <태그>를 모두 제거
        return Regex.Replace(source, "<.*?>", "");
    }
    public int MakeSyllable()
    {
        FinalWords = null;
        wordList = new List<int>();
        int childCount = transform.childCount;

        //카드의 위치를 바탕으로 CardSet의 가로 세로 크기 구하기

        int BatchColumn = batchBoard.BatchColumn;
        int minheight = 100, maxheight = -1, minwidth = 100, maxwidth = -1;
        for (int i = 0; i < childCount; i++)
        {
            CardCenter cardcenter = transform.GetChild(i).GetComponentInChildren<CardCenter>();
            int index = cardcenter.LastBatchPosIndex;
            Pos[0] = index / BatchColumn;
            Pos[1] = index % BatchColumn;
            //Debug.Log(Pos[1]+", "+Pos[0]);
            if (minheight > Pos[0])
            {
                minheight = Pos[0];
            }
            if (maxheight < Pos[0])
            {
                maxheight = Pos[0];
            }
            if (minwidth > Pos[1])
            {
                minwidth = Pos[1];
            }
            if (maxwidth < Pos[1])
            {
                maxwidth = Pos[1];
            }
        }
        int width = maxwidth - minwidth + 1, height = maxheight - minheight + 1;
        //Debug.Log("width: "+width+", height:  "+height);

        //2*2배열 크기 만들기
        Card[,] cardObj = new Card[width, height];
        for (int i = 0; i < childCount; i++)
        {
            CardCenter cardcenter = transform.GetChild(i).GetComponentInChildren<CardCenter>();
            int index = cardcenter.LastBatchPosIndex;
            Pos[0] = index / BatchColumn;
            Pos[1] = index % BatchColumn;
            cardObj[Pos[1] - minwidth, Pos[0] - minheight] = cardcenter.transform.parent.GetComponent<Card>();
        }
        //글자 확인 -> 자음 -> 쌍자음 확인 -> 모음 확인(ㅗ,ㅛ,ㅡ,ㅜ,ㅠ) -> 모음 확인(ㅓ,ㅏ,ㅣ,ㅕ,ㅑ) -> 2번째 모음 확인시 모음이 있으면 모음 확인(ㅏ,ㅓ,ㅣ)
        //-> 어절의 위치 저장, 다음 시작 초성의 위치 저장 -> 종성 확인

        //wordCount를 구하기기
        wordList.Add(0);
        int pastInfo = 0;   //전 단어의 WordKind가 자음이면 0
        try
        {
            if (cardObj[0, 0].WordKind == "vowel")
            {
                pastInfo = 1;
            }
        }
        catch
        {
            //Debug.LogError(e+" Shape processing failed.");
            IsSyllable = false;
            return 0;
        }
        for (int i = 1; i < width; i++)
        {
            if (cardObj[i, 0])
            {
                if (pastInfo == 0)
                {
                    if (cardObj[i, 0].WordKind == "vowel")
                    {
                        pastInfo = 1;
                    }
                    else if (cardObj[i - 1, 0].WordFactor != cardObj[i, 0].WordFactor)
                    {
                        wordList.Add(i);
                    }
                }
                else
                {
                    if (cardObj[i, 0].WordKind == "consonant")
                    {
                        pastInfo = 0;
                        wordList.Add(i);
                    }
                }
            }
        }
        wordList.Add(width);
        List<char> FinalWord = new List<char>();
        for (int index = 0; index < wordList.Count - 1; index++)
        {
            if (cardObj[wordList[index], height - 1])
            {
                FinalWord.Add(MakeWord(cardObj, wordList[index], wordList[index + 1], height));
            }
            else if (cardObj[wordList[index], height - 2])
            {
                FinalWord.Add(MakeWord(cardObj, wordList[index], wordList[index + 1], height - 1));
            }
            else
            {
                FinalWord.Add(MakeWord(cardObj, wordList[index], wordList[index + 1], height - 2));
            }
        }
        if (FinalWord.Contains('a'))
        {
            IsSyllable = false;
            return 0;
        }
        FinalWords = new string(FinalWord.ToArray());
        IsSyllable = true;
        //Debug.Log("Finally Made Syllable");
        return 0;
    }
    private char MakeWord(Card[,] cardObj, int index, int width, int height)
    {
        int chosungIndex = 0;
        int jungsungIndex = 0;
        List<string> jungsungList = new List<string>();
        int jongsungIndex = 0;
        List<string> jongsungList = new List<string>();
        int IsDoubleCho = 0;
        int IsUnderVow = 0;
        //자음, 쌍자음 확인
        //Debug.Log("cardObj: "+cardObj[index,0]);
        if (cardObj[index, 0].WordKind == "consonant")
        {
            if (index + 1 < width)
            {
                if (cardObj[index + 1, 0].WordFactor == cardObj[index, 0].WordFactor)
                {
                    IsDoubleCho++;
                    //쌍자음 확인
                    string[] temp = { cardObj[index, 0].WordFactor, cardObj[index, 0].WordFactor };
                    //Debug.Log(temp);
                    chosungIndex = FindIndex(Word_Chosung, temp);
                }
                else
                {
                    chosungIndex = FindIndex(Word_Chosung, new string[] { cardObj[index, 0].WordFactor });
                    //Debug.Log(new string [] {cardObj[index,0].WordFactor});
                }
            }
            else
            {
                chosungIndex = FindIndex(Word_Chosung, new string[] { cardObj[index, 0].WordFactor });
                //Debug.Log(new string [] {cardObj[index,0].WordFactor});
            }
        }

        //아래 모음 확인
        if (height > 1)
        {
            if (cardObj[index, 1].WordKind == "vowel")
            {
                IsUnderVow++;
                jungsungList.Add(cardObj[index, 1].WordFactor);
            }
        }

        //위 모음 확인
        if (index + 1 + IsDoubleCho < width)
        {
            jungsungList.Add(cardObj[index + 1 + IsDoubleCho, 0].WordFactor);
            //쌍 모음 확인
            if (index + 2 + IsDoubleCho < width)
            {
                jungsungList.Add(cardObj[index + 2 + IsDoubleCho, 0].WordFactor);
            }
        }
        string[] jungsungArray = new string[jungsungList.Count];
        for (int i = 0; i < jungsungList.Count; i++)
        {
            jungsungArray[i] = jungsungList[i];
        }
        jungsungIndex = FindIndex(Word_Jungsung, jungsungArray);

        //종성 확인
        if (height > 1 + IsUnderVow)
        {
            jongsungList.Add(cardObj[index, 1 + IsUnderVow].WordFactor);
            //쌍자음 확인
            if (width > index + 1)
            {
                if (cardObj[index + 1, 1 + IsUnderVow])
                {
                    jongsungList.Add(cardObj[index + 1, 1 + IsUnderVow].WordFactor);
                }
            }
        }
        else
        {
            jongsungList.Add(" ");
        }
        string[] jongsungArray = new string[jongsungList.Count];
        for (int i = 0; i < jongsungList.Count; i++)
        {
            jongsungArray[i] = jongsungList[i];
        }
        jongsungIndex = FindIndex(Word_Jongsung, jongsungArray);

        if (chosungIndex == -1 || jungsungIndex == -1 || jongsungIndex == -1)
        {
            //잘못된 Word
            //Debug.Log("error: "+ index);
            //Debug.Log($"cho: {chosungIndex}, jung: {jungsungIndex}, jong: {jongsungIndex}");
            return 'a';
        }
        else
        {
            //Debug.Log(ComposeHangul(Chosung[chosungIndex], Jungsung[jungsungIndex], Jongsung[jongsungIndex]));
            return ComposeHangul(Chosung[chosungIndex], Jungsung[jungsungIndex], Jongsung[jongsungIndex]);
        }
    }
    private char ComposeHangul(char chosung, char jungsung, char jongsung)
    {
        int chosungIndex = Array.IndexOf(Chosung, chosung);
        int jungsungIndex = Array.IndexOf(Jungsung, jungsung);
        int jongsungIndex = Array.IndexOf(Jongsung, jongsung);

        if (chosungIndex == -1 || jungsungIndex == -1 || jongsungIndex == -1)
        {
            IsSyllable = false;
        }

        // 한글 음절 유니코드 계산
        int hangulCode = HangulBase + (chosungIndex * ChosungBase) + (jungsungIndex * JungsungBase) + jongsungIndex;

        return (char)hangulCode;
    }
    private int FindIndex(string[][] array, string[] target)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (ArraysAreEqual(array[i], target))
            {
                return i;
            }
        }
        return -1; // 찾지 못했을 경우 -1 반환
    }
    private bool ArraysAreEqual(string[] array1, string[] array2)
    {
        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }
}
