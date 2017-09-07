// Interact Near Touch|Interactions|30055
namespace VRTK
{
    using UnityEngine;

    /// <summary>
    /// The Interact Near Touch script is usually applied to a Controller and provides a collider to know when the controller is nearly touching something.
    /// </summary>
    /// <remarks>
    /// Colliders are created for the controller and by default will be a sphere.
    ///
    /// A custom collider can be provided by the Custom Collider Container parameter.
    /// </remarks>
    [AddComponentMenu("VRTK/Scripts/Interactions/VRTK_InteractNearTouch")]
    public class VRTK_InteractNearTouch : MonoBehaviour
    {
        [Tooltip("The radius of the auto generated collider if a `Custom Collider Container` is not supplied.")]
        public float colliderRadius = 0.2f;
        [Tooltip("An optional GameObject that contains the compound colliders to represent the near touching object. If this is empty then the collider will be auto generated at runtime.")]
        public GameObject customColliderContainer;
        [Tooltip("The Interact Touch script to associate the near touches with. If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
        public VRTK_InteractTouch interactTouch;

        protected GameObject neartouchColliderContainer;

        /// <summary>
        /// Emitted when a valid object is near touched.
        /// </summary>
        public event ObjectInteractEventHandler ControllerNearTouchInteractableObject;
        /// <summary>
        /// Emitted when a valid object is no longer being near touched.
        /// </summary>
        public event ObjectInteractEventHandler ControllerNearUntouchInteractableObject;

        public virtual void OnControllerNearTouchInteractableObject(ObjectInteractEventArgs e)
        {
            if (ControllerNearTouchInteractableObject != null)
            {
                ControllerNearTouchInteractableObject(this, e);
            }
        }

        public virtual void OnControllerNearUntouchInteractableObject(ObjectInteractEventArgs e)
        {
            if (ControllerNearUntouchInteractableObject != null)
            {
                ControllerNearUntouchInteractableObject(this, e);
            }
        }

        protected virtual void OnEnable()
        {
            interactTouch = (interactTouch != null ? interactTouch : GetComponentInParent<VRTK_InteractTouch>());
            if (interactTouch != null)
            {
                CreateNearTouchCollider();
            }
        }

        protected virtual void OnDisable()
        {
            Destroy(neartouchColliderContainer);
        }

        protected virtual void CreateNearTouchCollider()
        {
            if (customColliderContainer == null)
            {
                neartouchColliderContainer = new GameObject();
                neartouchColliderContainer.transform.SetParent(interactTouch.transform);
                neartouchColliderContainer.transform.localPosition = Vector3.zero;
                neartouchColliderContainer.transform.localRotation = Quaternion.identity;
                neartouchColliderContainer.transform.localScale = interactTouch.transform.localScale;
            }
            else
            {
                neartouchColliderContainer = Instantiate(customColliderContainer, Vector3.zero, Quaternion.identity, interactTouch.transform);
            }

            neartouchColliderContainer.name = VRTK_SharedMethods.GenerateVRTKObjectName(true, "Controller", "NearTouch", "CollidersContainer");

            Rigidbody attachedRigidbody = neartouchColliderContainer.GetComponentInChildren<Rigidbody>();
            if (attachedRigidbody == null)
            {
                attachedRigidbody = neartouchColliderContainer.AddComponent<Rigidbody>();
            }

            attachedRigidbody.isKinematic = true;
            attachedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            Collider attachedCollider = neartouchColliderContainer.GetComponentInChildren<Collider>();
            if (attachedCollider == null)
            {
                SphereCollider attachedSphereCollider = neartouchColliderContainer.AddComponent<SphereCollider>();
                attachedSphereCollider.isTrigger = true;
                attachedSphereCollider.radius = colliderRadius;
            }
            else
            {
                attachedCollider.isTrigger = true;
            }

            VRTK_InteractNearTouchCollider interactNearTouchColliderScript = neartouchColliderContainer.AddComponent<VRTK_InteractNearTouchCollider>();
            interactNearTouchColliderScript.SetInteractNearTouch(this);

            neartouchColliderContainer.SetActive(true);
        }
    }

    public class VRTK_InteractNearTouchCollider : MonoBehaviour
    {
        protected VRTK_InteractNearTouch interactNearTouch;

        public virtual void SetInteractNearTouch(VRTK_InteractNearTouch givenInteractNearTouch)
        {
            interactNearTouch = givenInteractNearTouch;
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            VRTK_InteractableObject checkObject = collider.gameObject.GetComponentInParent<VRTK_InteractableObject>();
            if (validObject(checkObject))
            {
                if (checkObject != null)
                {
                    checkObject.StartNearTouching(interactNearTouch);
                }
                interactNearTouch.OnControllerNearTouchInteractableObject(interactNearTouch.interactTouch.SetControllerInteractEvent(collider.gameObject));
            }
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            VRTK_InteractableObject checkObject = collider.gameObject.GetComponentInParent<VRTK_InteractableObject>();
            if (validObject(checkObject))
            {
                if (checkObject != null)
                {
                    checkObject.StopNearTouching(interactNearTouch);
                }
                interactNearTouch.OnControllerNearUntouchInteractableObject(interactNearTouch.interactTouch.SetControllerInteractEvent(collider.gameObject));
            }
        }

        protected virtual bool validObject(VRTK_InteractableObject checkObject)
        {
            return (checkObject == null || checkObject.IsValidInteractableController(interactNearTouch.interactTouch.gameObject, checkObject.allowedNearTouchControllers));
        }
    }
}