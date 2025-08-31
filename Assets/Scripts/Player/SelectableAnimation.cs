using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopDownGame
{

    public class Selectable : MonoBehaviour
    {
		[Header("Outlets")]
		Animator anim;
		public LayerMask tileMask;

        void Start()
        {
			anim = GetComponent<Animator>();
        }

        // Detection for when mouse is hovering over selectable tiles; set to trigger hover animation
        void Update()
		{
			Vector3 mousePos = Input.mousePosition;
			Vector3 worldPos = GameController.instance.mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
			Vector2 rayDirection = Vector2.zero;
			
			RaycastHit2D hit = Physics2D.Raycast(worldPos, rayDirection, tileMask);
			if (hit.collider != null)
            {
				if (hit.collider.gameObject.tag == "SelectableTiles" &&
				new Vector3(Mathf.RoundToInt(worldPos.x / 2) * 2, Mathf.RoundToInt(worldPos.y / 2) * 2, 0) == transform.position)
				{
					anim.SetBool("Hovering", true);
				}
				else
				{
					anim.SetBool("Hovering", false);
					//print(hit.collider.gameObject);
				}
			}
            else
            {
				anim.SetBool("Hovering", false);
			}
		}
	}
}
