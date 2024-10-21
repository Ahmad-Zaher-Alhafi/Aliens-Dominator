﻿using Context;
using ManagersAndControllers;
using UnityEngine;

namespace UI {
    public class ControlsUI : MonoBehaviour {
        [SerializeField] private GameObject fpsViewButton;
        [SerializeField] private GameObject topDownViewButton;
        [SerializeField] private GameObject rocketsStrikeButton;

        private void Awake() {
            Ctx.Deps.EventsManager.ViewModeChanged += OnViewModeChanged;
        }

        private void OnViewModeChanged(GameController.ViewMode previousViewMode, GameController.ViewMode currentViewMode) {
            if (currentViewMode == GameController.ViewMode.General) {
                fpsViewButton.SetActive(false);
                topDownViewButton.SetActive(false);
                rocketsStrikeButton.SetActive(false);
                return;
            }

            fpsViewButton.SetActive(currentViewMode != GameController.ViewMode.FPS);
            topDownViewButton.SetActive(currentViewMode != GameController.ViewMode.TopDown);
            rocketsStrikeButton.SetActive(currentViewMode == GameController.ViewMode.TopDown);
        }

        private void OnDestroy() {
            Ctx.Deps.EventsManager.ViewModeChanged -= OnViewModeChanged;
        }
    }
}