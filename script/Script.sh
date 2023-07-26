#!/bin/bash
cd ..
for(( ; ; ))
do
echo - "Use 'report' to generate the report."
echo
echo - "Use 'slides' to generate the presentation."
echo
echo - "Use 'show_report' to show the report and generate it if does not exist."
echo
echo - "Use 'show_slides' to show the presentation and generate it if does not exist."
echo - "Use 'run' to run MOOGLE!."
echo
echo - "Use 'clean' to delete the useless files."
echo
echo - "Use 'exit' to close this program."
echo

read -p "Write a command: " C
echo $C
case $C in
	"slides")
		pdflatex Presentación/Presentación.tex
	;;
	"report")
		pdflatex Informe/Informe.tex
	;;
	"clean")
		rm *.aux
		rm *.log
		rm *.nav
		rm *.out
		rm *.snm
		rm *.toc
	;;
	"run")
		cd MoogleServer
		dotnet watch run
	;;
	"show_slides")
		echo
		read -p "- Name the program you want to open Presentacion.pdf with. Type 'start' to open it by default: " X

		if [ -e Presentación.pdf ]; then
			$X Presentación.pdf
		else
			pdflatex Presentación/Presentación.tex
			$X Presentación.pdf
		fi
	;;
	 "show_report")
		echo
		read -p "- Name the program you want to open Informe.pdf with. Type 'start' to open it by default: " Y
                if [ -e Informe.pdf ]; then
                       $Y Informe.pdf
                else
                        pdflatex Informe/Informe.tex
			$Y Informe.pdf
                fi
	;;
	"exit")
		exit
	;;
	*)
		echo "Unknown command."
		read -p "Press ENTER key to continue." G
	;;
esac
clear
done
