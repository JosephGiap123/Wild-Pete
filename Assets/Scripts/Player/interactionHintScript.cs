using UnityEngine;
using TMPro;
public class InteractionHintScript : MonoBehaviour
{

    [SerializeField] private TMP_Text textMeshProText;

    public void Awake()
    {
        this.gameObject.SetActive(false);
    }

    public void LateUpdate()
    {
        if (this.gameObject.activeSelf)
        {
            this.gameObject.transform.rotation = Quaternion.identity;
            // Counter the parent's scale flip to keep text always upright
            if (transform.parent != null)
            {
                float parentScaleX = transform.parent.localScale.x;
                // If parent is flipped (negative scale), flip child to cancel it out
                this.gameObject.transform.localScale = new Vector3(
                    parentScaleX != 0 ? Mathf.Sign(parentScaleX) : 1f,
                    1f,
                    1f
                );
            }
        }
    }
    public void ActivateHint(string interactionMessage)
    {
        this.gameObject.SetActive(true);
        if (string.IsNullOrEmpty(interactionMessage))
        {
            textMeshProText.text = ControlManager.instance.inputMapping[PlayerControls.Interact].ToString() + " to interact";
        }
        else
        {
            textMeshProText.text = ControlManager.instance.inputMapping[PlayerControls.Interact].ToString() + interactionMessage;
        }
    }

    public void DeactivateHint()
    {
        this.gameObject.SetActive(false);
    }
}
