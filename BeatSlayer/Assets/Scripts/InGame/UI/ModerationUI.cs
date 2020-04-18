using InGame.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModerationUI : MonoBehaviour
{
    public string ModerationFolder { get { return Application.persistentDataPath + "/data/moderation"; } }
    bool isLoading;

    private void Update()
    {
        if (!Directory.Exists(ModerationFolder)) return;

        IEnumerable<string> maps = Directory.GetFiles(ModerationFolder).Where(c => Path.GetExtension(c) == ".bsz");
        if(maps.Count() != 0 && !isLoading)
        {
            isLoading = true;
            GotoTest(maps.First());
        }
    }

    public void GotoTest(string filepath)
    {
        SceneloadParameters parameters = SceneloadParameters.ModerationPreset(filepath);
        SceneController.instance.LoadScene(parameters);
    }
}
