using LingeringHopes.Gameplay;
using LingeringHopes.UI;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LingeringHopes.Flow
{
    public class RestMode : GameMode
    {
        [SerializeField]
        ItemFactory itemFactory = null;

        [SerializeField, Range(0f, 1f), Header("Rest Phase")]
        float gatherSuccessPercentage = 0.3f;

        [SerializeField, Range(0, 10)]
        int maxItemGatheredPerRest = 1;

        [Header("UI Panels")]
        [SerializeField]
        RestPanel restPanel = null;

        [SerializeField]
        InventoryPanel inventoryPanel = null;

        [SerializeField]
        DetailPanel detailPanel = null;

        [Header("Rest Actions")]
        [SerializeField]
        RestAction sleepAction = null;

        [SerializeField]
        RestAction gatherFoodAction = null, progressAction = null;

        ItemUseContext useContext = null;

        protected override bool IsValid() => base.IsValid() && restPanel && inventoryPanel && detailPanel && itemFactory
            && sleepAction != null && gatherFoodAction != null && progressAction != null; 

        protected override void Start()
        {
            base.Start();

            sleepAction.BindToAction(Sleep);
            gatherFoodAction.BindToAction(GatherFood);
            progressAction.BindToAction(ProgressDuringNight);

            inventoryManager.OnItemAdded += AddItemToUI;
            inventoryManager.OnItemRemoved += RemoveItemFromUI;
            inventoryManager.OnItemsCleared += RemoveAllItemsFromUI;

            useContext = new ItemUseContext(satietyManager);
        }

        public override void EnableGameMode()
        {
            base.EnableGameMode();
            restPanel.OnSleepCalled += DisplaySleepAction;
            restPanel.OnGatherCalled += DisplayGatherFoodAction;
            restPanel.OnProgressCalled += DisplayProgressAction;

            inventoryPanel.OnItemSelected += DisplayItemDetail;
            detailPanel.OnItemAskedForConsumption += ConsumeItem;

            detailPanel.ClearDetails();
            restPanel.SetButtonsState(true);
        }

        void DisplaySleepAction()
        {
            detailPanel.DisplayActionDetail(sleepAction);
        }

        void DisplayGatherFoodAction()
        {
            detailPanel.DisplayActionDetail(gatherFoodAction);
        }

        void DisplayProgressAction()
        {
            detailPanel.DisplayActionDetail(progressAction);
        }

        void Sleep()
        {
            satietyManager.ResetTiredStack();
            satietyManager.ReduceSatiety();

            restPanel.SetButtonsState(false);
            EndRestMode();
        }

        void GatherFood()
        {
            for (int i = 0; i < maxItemGatheredPerRest; ++i)
            {
                if (Random.value <= gatherSuccessPercentage)
                {
                    IItem newItem = itemFactory.CreateRandomGatherableItem();
                    if (newItem != null)
                    {
                        inventoryManager.AddItem(newItem);
                    }
                }
            }

            restPanel.SetButtonsState(false);
            EndRestMode();
        }

        void ProgressDuringNight()
        {
            exitManager.ProgressThroughExit();
            satietyManager.IncrementTiredStack();
            satietyManager.ReduceSatiety();

            restPanel.SetButtonsState(false);
            EndRestMode();
        }

        void AddItemToUI(IItem item)
        {
            inventoryPanel.AddItemBox(item);
        }

        void RemoveItemFromUI(IItem item)
        {
            inventoryPanel.DeleteItemBox(item);
            detailPanel.ClearDetails();
        }

        void RemoveAllItemsFromUI()
        {
            inventoryPanel.DeleteAll();
            detailPanel.ClearDetails();
        }

        void DisplayItemDetail(IItem item)
        {
            detailPanel.DisplayItemDetail(item);
        }

        void ConsumeItem(IItem item)
        {
            inventoryManager.ConsumeItem(item, useContext);
        }

        void EndRestMode()
        {
            EndGameMode();
        }

        protected override void InterruptGameMode()
        {
            CallInteruptGameMode(new SavedDatas
            (
                inventoryManager.Items,
                satietyManager.CurrentSatiety,
                exitManager.CurrentProgress,
                false
            ));
        }

        public override void DisableGameMode()
        {
            base.DisableGameMode();

            if (!IsValid())
            {
                return;
            }

            restPanel.OnSleepCalled -= DisplaySleepAction;
            restPanel.OnGatherCalled -= DisplayGatherFoodAction;
            restPanel.OnProgressCalled -= DisplayProgressAction;

            inventoryPanel.OnItemSelected -= DisplayItemDetail;
            detailPanel.OnItemAskedForConsumption -= ConsumeItem;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (inventoryManager != null)
            {
                inventoryManager.OnItemAdded -= AddItemToUI;
                inventoryManager.OnItemRemoved -= RemoveItemFromUI;
            }
        }
    }
}

