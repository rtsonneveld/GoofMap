using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlyCam : MonoBehaviour
{
    public TMPro.TMP_Text hintText;

    private Vector3 speed;
    private Vector2 rotation;

    private Ray ray;
    private RaycastHit hit;

    private int raycastMask;

    // Start is called before the first frame update
    void Start()
    {
        raycastMask = 1<<LayerMask.NameToLayer("Triggers") | 1<<LayerMask.NameToLayer("Objects");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) {
            Cursor.lockState = CursorLockMode.Locked;
            speed = Vector2.zero;
        } else if (Input.GetMouseButtonUp(1)) {
            Cursor.lockState = CursorLockMode.None;
        }

        // Only move when RMB is held
        if (Input.GetMouseButton(1)) {

            hintText.gameObject.SetActive(false);

            speed.x += (Input.GetAxisRaw("Horizontal"));
            speed.z += (Input.GetAxisRaw("Vertical"));

            speed *= Mathf.Pow(0.99f, Time.deltaTime * 1000.0f);

            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");

            transform.eulerAngles = rotation;

            float multiplier = Input.GetKey(KeyCode.LeftShift) ? 4 : Input.GetKey(KeyCode.LeftControl) ? 0.25f : 1;
            transform.position = transform.position + (transform.rotation * speed * multiplier * Time.deltaTime);
        } else {

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f, GetComponent<Camera>().cullingMask & raycastMask)) {
                hintText.gameObject.SetActive(true);
                hintText.gameObject.transform.position = Camera.main.WorldToScreenPoint(hit.point);

                GameObject gao = hit.collider.gameObject;
                while(gao.transform.parent.transform.parent!=this.transform.parent) {
                    gao = gao.transform.parent.gameObject;
                }

                hintText.text = gao.name;
            } else {
                hintText.gameObject.SetActive(false);
            }
        }
    }

    private void ToggleOn(int layer, bool on)
    {
        if (on)
            GetComponent<Camera>().cullingMask |= 1 << layer;
        else
            GetComponent<Camera>().cullingMask &= ~(1 << layer);
    }

    public void ToggleObjects(bool on)
    {
        ToggleOn(LayerMask.NameToLayer("Objects"), on);
    }

    public void ToggleInvisibleGeometry(bool on)
    {
        ToggleOn(LayerMask.NameToLayer("InvisibleGeometry"), on);
    }

    public void ToggleTriggers(bool on)
    {
        ToggleOn(LayerMask.NameToLayer("Triggers"), on);
    }
}
