using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

namespace SimpleCharController
{
    [Serializable]
    public class BattleSystemEvents
    {
        public UnityEvent OnFire, OnAltFire, OffAltFire, CancelAltFire, OnWeaponSwitch;
    }
    public class SimpleInputActions : MonoBehaviour
    {
        [Header("Permission")]
        public bool canInput;

        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool moving;
        public bool jump;
        public bool sprint;
        //public bool activate;

        [Header("Weapon Input Values")]
        //public bool fire;
        //public bool altFire;
        public int selectedWeaponSlot = 1;
        [SerializeField] private int _weaponSlotsCount = 3;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        [Header("Battle Events")]
        [SerializeField] public BattleSystemEvents battleEvents;

        #region ============ Ã≈“Œƒ€ —»—“≈Ã€ ¬¬Œƒ¿ ============

        // ============ Ã≈“Œƒ€ ƒÀﬂ ƒ¬»∆≈Õ»ﬂ ============
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook && canInput)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            if (canInput) JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            if (canInput) SprintInput(value.isPressed);
        }

        // ============ Ã≈“Œƒ€ ƒÀﬂ Œ–”∆»ﬂ ============

        public void OnFire(InputValue value)
        {
            if (canInput) FireInput(value.isPressed);
        }

        public void OnAltFire(InputValue value)
        {
            if (canInput) AltFireInput(value.isPressed);
        }

        public void OnWeaponSlot1(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(1);
        }

        public void OnWeaponSlot2(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(2);
        }

        public void OnWeaponSlot3(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(3);
        }

        public void OnWeaponSlot4(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(4);
        }

        public void OnWeaponSlot5(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(5);
        }

        public void OnWeaponSlot6(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(6);
        }

        public void OnWeaponSlot7(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(7);
        }

        public void OnWeaponSlot8(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(8);
        }

        public void OnWeaponSlot9(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(9);
        }

        public void OnWeaponSlot0(InputValue value)
        {
            if (value.isPressed && canInput) SelectedWeaponSlotInput(0);
        }

        public void OnWeaponSwitchNext(InputValue value)
        {
            if (value.isPressed && canInput)
            {
                int nextSlot = selectedWeaponSlot + 1;
                if (nextSlot > _weaponSlotsCount) nextSlot = 1;
                SelectedWeaponSlotInput(nextSlot);
            }
        }

        public void OnWeaponSwitchPrev(InputValue value)
        {
            if (value.isPressed && canInput)
            {
                int prevSlot = selectedWeaponSlot - 1;
                if (prevSlot < 1) prevSlot = _weaponSlotsCount;
                SelectedWeaponSlotInput(prevSlot);
            }
        }

        public void OnWeaponSwitchScroll(InputValue value)
        {
            if (canInput)
            {
                Vector2 scrollDelta = value.Get<Vector2>();
                if (scrollDelta.y < 0)
                {
                    int nextSlot = selectedWeaponSlot + 1;
                    if (nextSlot > _weaponSlotsCount) nextSlot = 1;
                    SelectedWeaponSlotInput(nextSlot);
                }
                else if (scrollDelta.y > 0)
                {
                    int prevSlot = selectedWeaponSlot - 1;
                    if (prevSlot < 1) prevSlot = _weaponSlotsCount;
                    SelectedWeaponSlotInput(prevSlot);
                }
            }
        }

        #endregion

        #region ============ Ã≈“Œƒ€ Œ¡–¿¡Œ“ » ¬¬Œƒ¿ ==========

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
            moving = move.magnitude > 0;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void FireInput(bool newFireState)
        {
            if (newFireState) battleEvents.OnFire.Invoke();
        }

        public void AltFireInput(bool newAltFireState)
        {
            if (newAltFireState) battleEvents.OnAltFire?.Invoke();
            else battleEvents.OffAltFire?.Invoke();
        }

        public void SelectedWeaponSlotInput(int newSlotIndex)
        {
            if (ValidateSelectSlot(newSlotIndex))
            {
                selectedWeaponSlot = newSlotIndex;
                battleEvents.OnWeaponSwitch.Invoke();
            }
        }

        #endregion

        public void ResetValueInput()
        {
            look = Vector2.zero;
            jump = sprint = false;
            battleEvents.CancelAltFire?.Invoke();
        }

        private bool ValidateSelectSlot(int selectSlot)
        {
            if(selectSlot > _weaponSlotsCount || selectSlot < 1) return false;
            else return true;
        }
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}