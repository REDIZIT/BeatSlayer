using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AnimatedText : MonoBehaviour
{
    [TextArea]
    public string text;
    public bool isTyping;
    public int typingIndex;
    public int skipFrames;
    int skipFramesDelta;
    bool searchingForRichEnd;

    private void Start()
    {
        StartAnimate();
    }

    public void StartAnimate()
    {
        GetComponent<Text>().text = "";
        isTyping = true;
        typingIndex = 0;
    }

    private void Update()
    {
        if (text.Length <= typingIndex) isTyping = false;

        if (!isTyping) return;

        if(skipFramesDelta < skipFrames)
        {
            skipFramesDelta++;

            return;
        }

        if(text[typingIndex] == '<')
        {
            for (int i = typingIndex; i < text.Length - 1; i++)
            {
                if(text[i] == '>')
                {
                    GetComponent<Text>().text += text[i];
                    typingIndex++;
                    break;
                }
                else
                {
                    GetComponent<Text>().text += text[i];
                    typingIndex++;
                }
            }

            return;
        }

        GetComponent<Text>().text += text[typingIndex];
        typingIndex++;
        skipFramesDelta = 0;
    }
}
