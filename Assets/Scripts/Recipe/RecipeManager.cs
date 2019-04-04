using System;
using System.Collections.Generic;
using System.Linq;
using BensToolBox;
using UniRx;
using UnityEngine;

public class RecipeManager : Singleton<RecipeManager>
{
    private Recipe _recipeInProgress = null;
    private bool _waitingForBurner;
    public InstructionUI _instructionUi;
    
    public bool IsRecipeInProgress => _recipeInProgress != null;
    
    public void StartRecipe(Recipe recipe)
    {
        _recipeInProgress = recipe;
        
        _instructionUi.SetRecipe(recipe);
    }

    public Recipe UseBurner(BurnerBehaviour burner)
    {
        if (_recipeInProgress._burner != null) return null;
        
        _recipeInProgress._burner = burner;
        return _recipeInProgress;
    }

    public void EndRecipe()
    {
        _instructionUi.Hide();
        _instructionUi.transform.parent = this.transform;
        
        _recipeInProgress.FreeResources();
        _recipeInProgress = null;
    }
    public bool IsWaitingForBurner()
    {
        //if we are waiting for an assigned burner before moving to the next step, then we are waiting for a burner.
        return _recipeInProgress != null &&_recipeInProgress.CurrentStep.Value.NextStepTrigger == _recipeInProgress.HasAssignedBurner;
    }
    
    private void Update()
    {
        if (_recipeInProgress != null)
        {
            var isComplete = _recipeInProgress.Update();
            if(isComplete) EndRecipe();
        }
    }
}