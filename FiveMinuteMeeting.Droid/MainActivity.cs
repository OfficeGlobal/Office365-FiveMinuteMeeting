﻿using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FiveMinuteMeeting.Shared.ViewModels;
using FiveMinuteMeeting.Shared;
using Android.Support.V4.Widget;

namespace FiveMinuteMeeting.Droid
{
  [Activity(Label = "Five Minute Meeting", MainLauncher = true, Icon = "@drawable/ic_launcher")]
  public class MainActivity : ListActivity
  {

    private ContactsViewModel viewModel = App.ContactsViewModel;
    private SwipeRefreshLayout refresher;

    protected async override void OnCreate(Bundle bundle)
    {
      base.OnCreate(bundle);

      SetContentView(Resource.Layout.Main);

      refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
      refresher.SetColorScheme(Resource.Color.blue,
                                Resource.Color.white,
                                Resource.Color.blue,
                                Resource.Color.white);
      refresher.Refresh += async delegate
      {
        if(viewModel.IsBusy)
          return;

        await viewModel.GetContactsAsync();
        RunOnUiThread(() => { ((BaseAdapter)ListAdapter).NotifyDataSetChanged(); });
      };

      viewModel.PropertyChanged += PropertyChanged;
      ListAdapter = new ContactAdapter(this, viewModel);
      await Client.EnsureClientCreated(this);
      viewModel.GetContactsAsync();
    }

    protected override void OnListItemClick(ListView l, View v, int position, long id)
    {
      base.OnListItemClick(l, v, position, id);
      var contact = viewModel.Contacts[position];
      var vm = new DetailsViewModel(contact);
      DetailActivity.ViewModel = vm;
      var intent = new Intent(this, typeof(DetailActivity));
      StartActivity(intent);
    }


    public override bool OnCreateOptionsMenu(IMenu menu)
    {
      MenuInflater.Inflate(Resource.Menu.main, menu);
      return base.OnCreateOptionsMenu(menu);
    }


    public override bool OnOptionsItemSelected(IMenuItem item)
    {
      switch (item.ItemId)
      {
        case Resource.Id.add:
          DetailActivity.ViewModel = null;
           var intent = new Intent(this, typeof(DetailActivity));
          StartActivity(intent);
          break;
      }
      return base.OnOptionsItemSelected(item);
    }

    

    protected async override void OnResume()
    {
      base.OnResume();

      if(Client.Instance != null && viewModel.Contacts.Count == 0)
        viewModel.GetContactsAsync();
      else if (Client.Instance != null)
        RunOnUiThread(() => { ((BaseAdapter)ListAdapter).NotifyDataSetChanged(); });
    }

    void PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      RunOnUiThread(() =>
      {
        switch (e.PropertyName)
        {
          case "IsBusy":
            {
              refresher.Refreshing = viewModel.IsBusy;
              ((BaseAdapter)ListAdapter).NotifyDataSetChanged();
            }
            break;
        }
      });
    }
  }
}

