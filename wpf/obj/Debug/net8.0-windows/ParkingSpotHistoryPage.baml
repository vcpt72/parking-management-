   M S B A M L   `   `   `  ����  
ParkingWPF'    !ParkingWPF.ParkingSpotHistoryPage   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$"  Historie parkovacího místa��5   	   $	��450��6,   $	��800��69   .��5      � $��10q�6   .+�5   
   �� $(��"Historie stavu parkovacího místa��6   $��16(�6<   $
��Bold"�6J   $��0,0,0,10q�6\   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ HistoryGrid��5         AutoGenerateColumns$ False��6(   $	��300��6D      CanUserAddRows$ False��6Q      Columns 5	      0  *System.Windows.Controls.DataGridTextColumn  5
      ,  &System.Windows.Controls.DataGridColumn   Header$ Starý Stav��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6W   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *	OldStatus+6W     5      $ 
Nový Stav��6%   $
 Auto 6V    68   �� *	NewStatus+6V     5      $ Datum změny��6%   $
 Auto 6X    6:   �� *	ChangedAt+6X   5      5      5      