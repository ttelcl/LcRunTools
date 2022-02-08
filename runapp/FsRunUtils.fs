module FsRunUtils

// F# specific utilities related to Lcl.RunLib

open Lcl.RunLib.ApplicationDefinitions


let rec storeList (store: AppdefStore) =
  if store = null then
    []
  else
    store :: (store.Parent |> storeList)



