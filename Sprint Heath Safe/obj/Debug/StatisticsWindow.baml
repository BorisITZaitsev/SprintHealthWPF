   M S B A M L   `   `   `  ����G  CSprint Heath, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Sprint_HeathSprint_Heath  5   	   U QScottPlot.WPF, Version=5.0.55.0, Culture=neutral, PublicKeyToken=86698dc10387c39eD2clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPFScottPlot.WPF 5   	   #    Sprint_Heath.StatisticsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:Sprint_Heath     5   	   H	ScottPlot2clr-namespace:ScottPlot.WPF;assembly=ScottPlot.WPF     5   	     � Title$  SprintStatisticsWindow��5	   	   $	��450��6(   $	��800��65   .��5
      �  � ColumnDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5       � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5       .+�5"   
   �� 
�������$��8��6!   $��11��65   �� 5#   
   -   5         ��Name$ SprintTitle��5#      $-��'Some sprint name that I haven't got yet��6)   $��Center=�5$      $	��Top=�5%      $��30(�5&      $��Impact)�6"   
���   �$��1��5(      $��1��6$   $��6��61   $��2��6E   $��0,22,0,0q�6V   �� 5)   
   $��3��6   $��7��6$   $��6��65   $��
23,0,24,39q�6I   $��1��6]   .��5*      �� .+�5+        ScottPlot.WPF.WpfPlot  -   5          Name$ MyPlot��5+   $   $	��500��64   $	��220��6@   �� 5,      -   5        $ OutputTextBlock1��5,      $��20(�66   $��Fonts\#Oswald)�6D   $	��450��6_   $��Center=�6k    �� TextAlignment$ Justify=�6�   $��WrapWithOverflow=�6�   �� 5-      -   5        $ OutputTExtblock2��5-      $��20(�66   $��Fonts\#Oswald)�6D   $	��450��6`   $��Center=�6l   $ Justify=�6�   $��WrapWithOverflow=�6�   �� 5.      -   5        $ OutputTExtblock3��5.      $��20(�66   $��Fonts\#Oswald)�6D   $	��450��6`   $��Center=�6l   $ Justify=�6�   $��WrapWithOverflow=�6�   �� 5/      -   5        $ OutputTExtblock4��5/      $��20(�66   $��Fonts\#Oswald)�6D   $	��450��6`   $��Center=�6l   $ Justify=�6�   $��WrapWithOverflow=�6�   �� 50      -   5        $��3��50      $��9��6)   $��2��66   $��Fonts\#Oswald)�51   	   $#��Learn more about distribution��52   	   
�������
���/kU�
���/kU�6}   53      54      �� 55   
   -   5        $��3��55      $��9��6!   $��2��6.   $��Fonts\#Oswald)�56      $
��Back��57      
�������
����� �
����� �$��	40,2,40,8q�58      �� 59   
   $	��800��6   $	��450��6    $��Center=�6-   $��Center=�6J   $��0��5:      $��0��6$   $��8��5;      $��11��6(   ��5<      �� 5=       �� ImageSource$( "Images\HK background startpage.png��6    J� Clip 5?      � 5@      .��5A      ��  �� Rect$ 0,0,800,450�6(   	 �� RadiusX$	 0Q�6;   
 �� RadiusY$
 0Q�6G   �� 5B      $ 114,45,570,356�6(   $	 20Q�6>   $
 20Q�6K   5C      5E      5F      5G      