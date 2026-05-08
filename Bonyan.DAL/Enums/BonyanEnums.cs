using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.DAL.Enums
{
	public enum Gender
	{
		Male,
		Female
	}

	public enum Specialization
	{
		Civil,
		Mechanical,
		Electrical,
		Architectural,
	}

	public enum UserRole
	{
		Admin,
		Manager,
		Engineer
	}

	public enum ProjectStatus
	{
		Planning,
		InProgress,
		OnHold,
		Completed,
		Cancelled
	}

	public enum TasksStatus
	{
		Pending,
		InProgress,
		UnderReview,
		Completed,
		Cancelled
	}

	public enum SafetyStatus
	{
		Safe,
		NeedAttention,
		Critical
	}

	public enum InvoiceStatus
	{
		Paid,
		Unpaid,
		PartiallyPaid
	}

	public enum PaymentMethod
	{
		Cash,
		CreditCard,
		BankTransfer
	}

	public enum PaymentStatus
	{
		Completed,
		Pending,
		Failed
	}

	public enum MaterialTaskStatus
	{
		Requested,
		Approved,
		Delivered
	}
	public enum MaterialUnitTypes
	{
		Kg,
		Ton,
		Meter,
		SquareMeter,
		CubicMeter,
		Piece,
	}
	public enum MaterialStatusType
	{
		Available,
		Discontinued
	}
}
