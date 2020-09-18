using InGame.Game.Sabers;
using InGame.ScriptableObjects;
using InGame.Settings;
using UnityEngine;

public class SaberController : MonoBehaviour
{
    [SerializeField] private MeleeWeaponTrail trail;
    [SerializeField] private SODB sodb;
    [SerializeField] private Transform handPoint;


    public float camOffset, camScale;

    private GameObject model;
    private SaberModelController modelController;

    private Camera cam;
    private bool hideSabers;



    private void Start()
    {
        cam = Camera.main;
    }


    public void SetSword(Vector3 pos)
    {
        if (hideSabers) return;

        if (!model.activeSelf)
        {
            trail.DontUseThisFrame = true;
        }

        bool isVerticalMode = Screen.height > Screen.width;

        float xScale = isVerticalMode ? camScale / 2f : camScale;
        float yScale = isVerticalMode ? camScale : camScale / 2f;

        // Applied only in vertical mode
        Vector3 verticalOffset = isVerticalMode ? new Vector3(0, 0.06f) : Vector3.zero;

        transform.localPosition = cam.ScreenToViewportPoint(new Vector3((pos.x - Screen.width / 2f) * xScale, (pos.y - Screen.height / 2f) * yScale) + new Vector3(0, 0, camOffset));
        transform.localPosition += verticalOffset;
        transform.LookAt(handPoint);
    }


    public void Init(Color clr, int skinId, int swooshEffectId, float lifetime = 0.2f)
    {
        hideSabers = SettingsManager.Settings.Gameplay.HideSabers;
        if (hideSabers) return;

        trail._colors[0] = clr;
        trail._material = sodb.tails[swooshEffectId].swooshMaterial;
        trail._lifeTime = lifetime;


        ShowModel(sodb.sabers[skinId], clr);

        trail._base = modelController.trailBottom;
        trail._tip = modelController.trailTop;
    }

    public void SetEnabled(bool enabled)
    {
        if (hideSabers) return;

        model.SetActive(enabled);
    }


    private void ShowModel(SaberSO saber, Color color)
    {
        if (model != null)
        {
            Destroy(model);
        }

        model = Instantiate(saber.model);
        model.transform.SetParent(transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localEulerAngles = Vector3.zero;

        modelController = model.GetComponent<SaberModelController>();

        modelController.SetColor(color);
    }
}